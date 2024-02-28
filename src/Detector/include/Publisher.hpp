#pragma once

#include <unordered_map>
#include <string>

#include "rigtorp/MPMCQueue.h"
#include "nlohmann/json.hpp"

#include "MessagePublisher.hpp"
#include "DNSPacketInfo.hpp"
#include "ValidatedDomains.hpp"

extern std::atomic<bool> cancellation_token;

class Publisher
{
public:
	explicit Publisher(rigtorp::MPMCQueue<ValidatedDomains> *publisher_queue, MessagePublisher *message_publisher)
		: publisher_queue_(publisher_queue), message_publisher_(message_publisher) {}

	void Process() const;

private:
	rigtorp::MPMCQueue<ValidatedDomains> *publisher_queue_;
	MessagePublisher *message_publisher_;

	static nlohmann::json ToJson(const std::unordered_map<std::string, int> &domain_return_code_pairs);
};