#pragma once

#include "cxxopts.hpp"

#include "Exceptions.hpp"
#include "ReturnCodes.hpp"

class arguments
{
public:
	void parse(int argc, const char* argv[]);

	std::string interface_to_sniff;

	int packet_buffer_size = 0;

private:
	void check_rabbit_mq_connection(const std::string &rabbitMqConnectionString);
};
