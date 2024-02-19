#pragma once

#include <iostream>

#include "cxxopts.hpp"

#include "Error.hpp"

class arguments
{
public:
	ResultCode parse(const int argc, const char *argv[]);

	std::string Interface;

private:
	void check_rabbit_mq_connection(const std::string& rabbitMqConnectionString);
};
