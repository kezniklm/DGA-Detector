#pragma once

#include <limits>

#include "cxxopts.hpp"

#include "Exceptions.hpp"
#include "Packet.hpp"
#include "ReturnCodes.hpp"

#undef max

class arguments
{
public:
	void parse(int argc, const char *argv[]);

	std::string interface_to_sniff;

	int packet_buffer_size = 0;

	size_t packet_queue_size = 0;

private:
	void check_rabbit_mq_connection(const std::string &rabbitMqConnectionString);
};
