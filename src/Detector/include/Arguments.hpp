#pragma once

#include <fstream>
#include <iostream>
#include <algorithm>
#include <limits>

#include "cxxopts.hpp"
#include "nlohmann/json.hpp"

#include "DNSPacketInfo.hpp"
#include "Exceptions.hpp"
#include "Packet.hpp"
#include "ReturnCodes.hpp"

#ifdef _WIN32
#undef max
#endif

class Arguments
{
public:
	void Parse(const int argc, const char *argv[]);

	std::string interface_to_sniff_;

	unsigned long long memory_size_;

	std::string database_connection_string_;

	std::string rabbitmq_connection_string_;

	std::string rabbitmq_queue_name_;

	int packet_buffer_size_;

	size_t packet_queue_size_;

	size_t dns_info_queue_size_;

	size_t publisher_queue_size_;

private:
	void ConfigureOptions(cxxopts::Options &options);

	nlohmann::json LoadAppSettings();

	void ValidateAndSetOptions(const cxxopts::ParseResult &result, const nlohmann::json &appsettings, const cxxopts::Options &options);

	template <typename T>
	void SetOption(const std::string &key, T &value, const cxxopts::ParseResult &result, const nlohmann::json &appsettings, bool required, const cxxopts::Options &options);

	void CalculateSizes(const unsigned long long value);

	nlohmann::json MakeKeysLowercase(const nlohmann::json &original);
};
