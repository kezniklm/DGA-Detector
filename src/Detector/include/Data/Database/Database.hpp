#pragma once

#include <string>

class Database
{
public:
	virtual void HandleBlacklistHit(const std::string &element) = 0;
	virtual std::map<std::string, bool> CheckInBlacklist(const std::unordered_set<std::string> &elements) = 0;
	virtual std::map<std::string, bool> CheckInWhitelist(const std::unordered_set<std::string> &elements) = 0;

	virtual ~Database() = default;
};
