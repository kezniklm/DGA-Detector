#pragma once

#include <atomic>
#include <chrono>
#include <thread>
#include <unordered_map>
#include <unordered_set>
#include <map>

#include <rigtorp/MPMCQueue.h>

#include "Database.hpp"
#include "DNSPacketInfo.hpp"
#include "MessagePublisher.hpp"
#include "ValidatedDomains.hpp"

extern std::atomic<bool> cancellation_token;

class DomainValidator
{
public:
	explicit DomainValidator(rigtorp::MPMCQueue<DNSPacketInfo> *dns_info_queue,
							 rigtorp::MPMCQueue<ValidatedDomains> *publisher_queue,
							 Database *db)
		: dns_info_queue_(dns_info_queue), publisher_queue_(publisher_queue), database_(db) {}

	void ProcessDomains();

private:
	void ProcessPacketInfo(const DNSPacketInfo &packetInfo, std::unordered_map<std::string, int> &domainReturnCodePairs, int &cycleCount);
	bool ShouldProcessBatch(size_t currentBatchSize, int cycleCount, int maxBatchSize, int maxCycleCount) const;
	void ProcessBatch(std::unordered_map<std::string, int> &domainReturnCodePairs);
	void RemoveListedDomains(std::unordered_map<std::string, int> &domainReturnCodePairs, const std::map<std::string, bool> &resultList);

	rigtorp::MPMCQueue<DNSPacketInfo> *dns_info_queue_;
	rigtorp::MPMCQueue<ValidatedDomains> *publisher_queue_;
	Database *database_;
};
