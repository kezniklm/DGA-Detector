#pragma once

#include <string>

class Database
{
public:
	virtual void HandleBlacklistHit(const std::string &element) = 0;
	virtual bool CheckInBlacklist(const std::string &element) = 0;
	virtual bool CheckInWhitelist(const std::string &element) = 0;

	virtual ~Database() = default;
};

