#include "DomainValidator.hpp"

using namespace std;

void DomainValidator::ProcessDomains()
{
	constexpr int DEFAULT_SIZE = 100000;
	unordered_map<string, int> domainReturnCodePairs;
	domainReturnCodePairs.reserve(DEFAULT_SIZE);

	constexpr int MAX_BATCH_SIZE = DEFAULT_SIZE;
	constexpr int MAX_CYCLE_COUNT = DEFAULT_SIZE;

	int cycleCount = 0;

	while (!cancellation_token.load())
	{
		DNSPacketInfo packetInfo;
		if (dns_info_queue_->try_pop(packetInfo))
		{
			ProcessPacketInfo(packetInfo, domainReturnCodePairs, cycleCount);

			if (ShouldProcessBatch(domainReturnCodePairs.size(), cycleCount, MAX_BATCH_SIZE, MAX_CYCLE_COUNT))
			{
				ProcessBatch(domainReturnCodePairs);
				cycleCount = 0; // Reset cycle count for the next batch
			}
		}
		else
		{
			this_thread::sleep_for(chrono::milliseconds(100)); // Add sleep to reduce CPU usage
		}
	}
}

void DomainValidator::ProcessPacketInfo(const DNSPacketInfo &packetInfo, unordered_map<string, int> &domainReturnCodePairs, int &cycleCount)
{
	for (const auto &domainName : packetInfo.domain_names)
	{
		domainReturnCodePairs[domainName] = packetInfo.response_code;
		cycleCount++;
	}
}

bool DomainValidator::ShouldProcessBatch(size_t currentBatchSize, int cycleCount, int maxBatchSize, int maxCycleCount) const
{
	return currentBatchSize >= maxBatchSize || cycleCount > maxCycleCount;
}

void DomainValidator::ProcessBatch(std::unordered_map<std::string, int> &domainReturnCodePairs)
{
	std::unordered_set<std::string> domainNamesToQuery;
	for (const auto &pair : domainReturnCodePairs)
	{
		domainNamesToQuery.insert(pair.first);
	}

	auto resultBlacklistCheck = database_->CheckInBlacklist(domainNamesToQuery);
	auto resultWhitelistCheck = database_->CheckInWhitelist(domainNamesToQuery);

	RemoveListedDomains(domainReturnCodePairs, resultBlacklistCheck);
	RemoveListedDomains(domainReturnCodePairs, resultWhitelistCheck);

	publisher_queue_->emplace(ValidatedDomains{domainReturnCodePairs}); // Assuming ValidatedDomains can be constructed this way

	domainReturnCodePairs.clear();
}

void DomainValidator::RemoveListedDomains(std::unordered_map<std::string, int> &domainReturnCodePairs, const std::map<std::string, bool> &resultList)
{
	for (const auto &result : resultList)
	{
		if (result.second)
		{
			domainReturnCodePairs.erase(result.first);
		}
	}
}