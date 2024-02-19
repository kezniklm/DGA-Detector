#pragma once

#include "cxxopts.hpp"

#include "Error.hpp"

class arguments
{
public:
	ResultCode parse(int argc, const char* argv[]);

	std::string interface_to_sniff;

private:
	void check_rabbit_mq_connection(const std::string& rabbitMqConnectionString);
};
