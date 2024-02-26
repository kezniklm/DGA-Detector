#pragma once

#include <rigtorp/MPMCQueue.h>
#include "MessagePublisher.hpp"
#include "DNSPacketInfo.hpp"

#include "nlohmann/json.hpp"

extern std::atomic<bool> cancellation_token;

class Publisher
{
public:
	explicit Publisher(rigtorp::MPMCQueue<DNSPacketInfo> *publisher_queue, MessagePublisher *message_publisher)
		: publisher_queue_(publisher_queue), message_publisher_(message_publisher) {}

	void Process();

private:
	rigtorp::MPMCQueue<DNSPacketInfo> *publisher_queue_;
	MessagePublisher *message_publisher_;

	static nlohmann::json ToJson(const DNSPacketInfo &packet_info);
};