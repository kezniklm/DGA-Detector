#pragma once

#include "cxxopts.hpp"

#include "Error.hpp"

class arguments
{
public:
	ResultCode parse(int argc, const char *argv[]);

	std::string interface_to_sniff;

	int packet_buffer_size;

private:
	void check_rabbit_mq_connection(const std::string &rabbitMqConnectionString);
};
