/**
 * @file MongoDbDatabase.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Declaration of the MongoDbDatabase class for interacting with MongoDB.
 *
 * The main components of this file include:
 * - Declaration of the MongoDbDatabase class, responsible for interacting with MongoDB, performing queries, and handling blacklist hits.
 * - Declaration of member variables to store MongoDB connection details and database references.
 * - Declaration of member functions for checking the connection to the MongoDB server, performing queries with retries, handling blacklist hits, and checking elements against the blacklist and whitelist.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 *
 */

#pragma once

#include <chrono>
#include <iostream>
#include <map>
#include <string>
#include <thread>
#include <unordered_set>

#include <bsoncxx/json.hpp>
#include <bsoncxx/types.hpp>
#include <mongocxx/client.hpp>
#include <mongocxx/instance.hpp>
#include <mongocxx/uri.hpp>
#include <mongocxx/exception/exception.hpp>

#include "IDatabase.hpp"
#include "Logger.hpp"

/** Declare external cancellation token */
extern std::atomic<bool> cancellation_token;

/** Declare external logger pointer */
extern Logger *global_logger_ptr;

/**
 * @class MongoDbDatabase
 * @brief Interacts with MongoDB for database operations.
 */
class MongoDbDatabase : public IDatabase
{
public:
    /**
     * @brief Constructs a MongoDbDatabase object.
     * @param uri The MongoDB connection URI.
     * @param db_name The name of the database.
     */
    MongoDbDatabase(const std::string &uri, const std::string &db_name) : connection_{mongocxx::uri{uri}}
    {
        db_ = connection_[db_name];
        CheckConnection();
    }

    /**
     * @brief Checks elements against the blacklist.
     * @param elements The elements to check.
     * @return A map indicating whether each element is in the blacklist.
     */
    std::map<std::string, bool> CheckInBlacklist(const std::unordered_set<std::string> &elements) override
    {
        try
        {
            return CheckInList("Blacklist", elements);
        }
        catch (const std::exception &e)
        {
            global_logger_ptr->critical(std::string("Error: " + std::string(e.what()) + "\n"));
            cancellation_token.store(true);
            return std::map<std::string, bool>{};
        }
    }

    /**
     * @brief Checks elements against the whitelist.
     * @param elements The elements to check.
     * @return A map indicating whether each element is in the whitelist.
     */
    std::map<std::string, bool> CheckInWhitelist(const std::unordered_set<std::string> &elements) override
    {
        try
        {
            return CheckInList("Whitelist", elements);
        }
        catch (const std::exception &e)
        {
            global_logger_ptr->critical(std::string("Error: " + std::string(e.what()) + "\n"));
            cancellation_token.store(true);
            return std::map<std::string, bool>{};
        }
    }

private:
    /**
     * @brief Checks elements against a specified list (blacklist or whitelist).
     * @param list_name The name of the list to check.
     * @param elements The elements to check.
     * @return A map indicating whether each element is in the specified list.
     */
    std::map<std::string, bool> CheckInList(const std::string &list_name,
                                            const std::unordered_set<std::string> &elements)
    {
        auto query_func = [this, &list_name, &elements]() -> std::map<std::string, bool>
        {
            std::map<std::string, bool> results;

            auto collection = db_[list_name];
            bsoncxx::builder::basic::array elements_array;
            for (const auto &kElement : elements)
            {
                elements_array.append(kElement);
            }

            const auto filter = bsoncxx::builder::basic::make_document(
                bsoncxx::builder::basic::kvp("element",
                                             bsoncxx::builder::basic::make_document(
                                                 bsoncxx::builder::basic::kvp("$in", elements_array))));

            auto cursor = collection.find(filter.view());

            // Initialize results with false
            for (const auto &kElement : elements)
            {
                results[kElement] = false;
            }

            for (auto &&doc : cursor)
            {
                std::string element = bsoncxx::to_string(doc["element"].type());
                // Use the element string as needed
                results[element] = true;
                if (list_name == "Blacklist")
                {
                    HandleBlacklistHit(element);
                }
            }

            return results;
        };

        return PerformWithRetries(query_func, 3, 1000); // 3 retries, 1000ms delay
    }

    /**
     * @brief Checks the connection to the MongoDB server.
     * @throws std::runtime_error if the ping to the MongoDB server fails.
     */
    void CheckConnection() const
    {
        try
        {
            auto admin = connection_["admin"];
            const auto kCommand = bsoncxx::builder::basic::make_document(bsoncxx::builder::basic::kvp("ping", 1));
            admin.run_command(kCommand.view());
        }
        catch (const mongocxx::exception &e)
        {
            throw std::runtime_error(std::string("Failed to ping MongoDB server: ") + e.what());
        }
    }

    /**
     * @brief Performs a query with retries in case of failure.
     * @param query_func The function representing the query.
     * @param max_retries The maximum number of retries.
     * @param retry_delay_ms The delay between retries in milliseconds.
     * @return The result of the query function.
     * @throws std::runtime_error if the query fails after all retries.
     */
    template <typename Func>
    auto PerformWithRetries(Func query_func, const int max_retries, const int retry_delay_ms) -> decltype(query_func())
    {
        for (int attempt = 0; attempt < max_retries; ++attempt)
        {
            try
            {
                return query_func(); // Attempt the function and return result directly if successful
            }
            catch (const mongocxx::exception &e)
            {
                std::cerr << "MongoDB query failed: " << e.what();
                if (attempt < max_retries - 1)
                {
                    std::cerr << " - Retrying..." << std::endl;
                    std::this_thread::sleep_for(std::chrono::milliseconds(retry_delay_ms));
                }
                else
                {
                    std::cerr << " - All retries exhausted." << std::endl;
                    throw; // Rethrow the last exception
                }
            }
        }
        throw std::runtime_error("Failed after retries");
    }

    /**
     * @brief Handles a hit in the blacklist by inserting the element and timestamp into the "Results" collection.
     * @param element The element that hit the blacklist.
     */
    void HandleBlacklistHit(const std::string &element) override
    {
        const auto kNow = std::chrono::system_clock::now();
        const auto timestamp = std::chrono::duration_cast<std::chrono::seconds>(kNow.time_since_epoch()).count();

        auto collection = db_["Results"];
        bsoncxx::builder::basic::document document{};
        document.append(bsoncxx::builder::basic::kvp("element", element),
                        bsoncxx::builder::basic::kvp("timestamp", static_cast<int64_t>(timestamp)));

        collection.insert_one(document.view());
    }

    /** MongoDB driver instance */
    mongocxx::instance instance_{};

    /** MongoDB connection */
    mongocxx::client connection_;

    /** Database reference */
    mongocxx::database db_;
};