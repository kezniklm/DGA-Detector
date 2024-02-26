#pragma once

#include <fstream>
#include <limits>

#include "cxxopts.hpp"
#include "nlohmann/json.hpp"

#include "DNSPacketInfo.hpp"
#include "Exceptions.hpp"
#include "Packet.hpp"
#include "ReturnCodes.hpp"

#undef max

class Arguments
{
public:
	void Parse(int argc, const char *argv[]);

	std::string interface_to_sniff_;

	std::string database_connection_string_;

	std::string rabbitmq_connection_string_;

	std::string rabbitmq_queue_name_;

	int packet_buffer_size_ = 0;

	size_t packet_queue_size_ = 0;

	size_t dns_info_queue_size_ = 0;

	size_t publisher_queue_size_ = 0;

private:
	void CalculateSizes(unsigned long long value);
	static nlohmann::json MakeKeysLowercase(const nlohmann::json &original);
};
