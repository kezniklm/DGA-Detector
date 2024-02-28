#pragma once

#include <vector>
#include <string>
#include <iostream>

struct DNSPacketInfo
{
public:
	std::vector<std::string> domain_names;
	int response_code{};

	DNSPacketInfo(const std::vector<std::string> &domains, int code)
		: domain_names(domains), response_code(code) {}

	DNSPacketInfo() = default;
};