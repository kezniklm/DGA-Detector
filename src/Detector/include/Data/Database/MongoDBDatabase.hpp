#pragma once

#include <mongocxx/client.hpp>
#include <mongocxx/instance.hpp>
#include <mongocxx/uri.hpp>
#include <mongocxx/exception/exception.hpp>
#include <iostream>
#include <string>
#include "Database.hpp"

class MongoDBDatabase : public Database
{
private:
	mongocxx::instance instance_{}; // MongoDB driver instance_
	mongocxx::client connection_;   // MongoDB connection_
	mongocxx::database db_;         // Database reference

	void CheckConnection()
	{
		try
		{
			auto admin = connection_["admin"];
			auto command = bsoncxx::builder::basic::make_document(bsoncxx::builder::basic::kvp("ping", 1));
			admin.run_command(command.view());
		}
		catch (const mongocxx::exception &e)
		{
			throw DetectorException(std::string("Failed to ping MongoDB server: ") + e.what(), FAILURE);
		}
	}

	template<typename Func>
	bool PerformWithRetries(Func query_func, int max_retries, int retry_delay_ms)
	{
		for (int attempt = 0; attempt < max_retries; ++attempt)
		{
			try
			{
				if (query_func()) // Attempt the function
				{
					return true; // Success
				} else
				{
					return false; // Document not found, no need to retry
				}
			}
			catch (const mongocxx::exception &e)
			{
				std::cerr << "MongoDB query failed: " << e.what();
				if (attempt < max_retries - 1)
				{
					std::cerr << " - Retrying..." << std::endl;
					std::this_thread::sleep_for(std::chrono::milliseconds(retry_delay_ms));
				} else
				{
					std::cerr << " - All retries exhausted." << std::endl;
				}
			}
		}
		return false; // Failure after retries
	}

	void HandleBlacklistHit(const std::string &element) override
	{
		auto now = std::chrono::system_clock::now();
		auto timestamp = std::chrono::duration_cast<std::chrono::seconds>(now.time_since_epoch()).count();

		auto collection = db_["Results"];
		bsoncxx::builder::basic::document document{};
		document.append(bsoncxx::builder::basic::kvp("element", element),
						bsoncxx::builder::basic::kvp("timestamp", timestamp));

		collection.insert_one(document.view());
	}

public:
	MongoDBDatabase(const std::string &uri, const std::string &db_name) : connection_{mongocxx::uri{uri}}
	{
		db_ = connection_[db_name];
		CheckConnection();
	}

	bool CheckInBlacklist(const std::string &element) override
	{
		auto query_func = [this, &element]()
		{
			auto collection = db_["Blacklist"];
			auto maybe_result = collection.find_one(bsoncxx::builder::basic::make_document(bsoncxx::builder::basic::kvp(
				"element",
				element)));
			if (maybe_result)
			{
				HandleBlacklistHit(element); // Log the blacklist hit
			}
			return static_cast<bool>(maybe_result);
		};
		return PerformWithRetries(query_func, 3, 1000); // 3 retries, 1000ms delay
	}

	bool CheckInWhitelist(const std::string &element) override
	{
		auto query_func = [this, &element]()
		{
			auto collection = db_["Whitelist"];
			auto maybe_result = collection.find_one(bsoncxx::builder::basic::make_document(bsoncxx::builder::basic::kvp(
				"element",
				element)));
			return static_cast<bool>(maybe_result); // Adjusted here
		};
		return PerformWithRetries(query_func, 3, 1000); // 3 retries, 1000ms delay
	}

};
