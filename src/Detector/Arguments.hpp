#pragma once

#include <string>


class arguments
{
public:
	void parse(const int argc, const char* argv[]);

private:
	void check_rabbit_mq_connection(const std::string& rabbitMqConnectionString);
};
