#pragma once

#include <iostream>
#include <string>

#include <mongocxx/client.hpp>
#include <mongocxx/exception/exception.hpp>
#include <mongocxx/instance.hpp>
#include <mongocxx/uri.hpp>
#include <mongocxx/exception/exception.hpp>
#include <iostream>
#include <string>
#include <vector>
#include <map>
#include <chrono>
#include <thread>
#include "Database.hpp"
#include <bsoncxx/json.hpp>                       // Include BSON support
#include <bsoncxx/types.hpp>                      // Include BSON types
#include <mongocxx/exception/query_exception.hpp> // Include specific exception types
#include <unordered_set>

class MongoDbDatabase : public Database
{
private:
    mongocxx::instance instance_{}; // MongoDB driver instance
    mongocxx::client connection_;   // MongoDB connection
    mongocxx::database db_;         // Database reference

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

    template <typename Func>
    auto PerformWithRetries(Func query_func, int max_retries, int retry_delay_ms) -> decltype(query_func())
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

public:
    MongoDbDatabase(const std::string &uri, const std::string &db_name) : connection_{mongocxx::uri{uri}}
    {
        db_ = connection_[db_name];
        CheckConnection();
    }

    std::map<std::string, bool> CheckInBlacklist(const std::unordered_set<std::string> &elements) override
    {
        return CheckInList("Blacklist", elements);
    }

    std::map<std::string, bool> CheckInWhitelist(const std::unordered_set<std::string> &elements) override
    {
        return CheckInList("Whitelist", elements);
    }

private:
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
};