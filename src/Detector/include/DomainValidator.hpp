#pragma once

#include "Database.hpp"
#include "DNSPacketInfo.hpp"
#include "MessagePublisher.hpp"
#include <rigtorp/MPMCQueue.h>

extern std::atomic<bool> cancellation_token;

class DomainValidator
{
public:
	explicit DomainValidator(rigtorp::MPMCQueue<DNSPacketInfo> *dns_info_queue,
							 rigtorp::MPMCQueue<DNSPacketInfo> *publisher_queue,
							 Database *db)
		: dns_info_queue_(dns_info_queue), publisher_queue_(publisher_queue), database_(db) {}

	void ProcessDomains();

private:
	rigtorp::MPMCQueue<DNSPacketInfo> *dns_info_queue_;

	rigtorp::MPMCQueue<DNSPacketInfo> *publisher_queue_;

	Database *database_;
};