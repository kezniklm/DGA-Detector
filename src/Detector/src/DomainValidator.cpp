#include "DomainValidator.hpp"

void DomainValidator::ProcessDomains()
{
	DNSPacketInfo packet_info;
	while (!cancellation_token.load())
	{
		if (dns_info_queue_->try_pop(packet_info))
		{
			bool found = false;

			for (const auto &kDomainName : packet_info.domain_names)
			{
				if (database_->CheckInBlacklist(kDomainName) || database_->CheckInWhitelist(kDomainName))
				{
					found = true;
				}
			}

			if (!found)
			{
				publisher_queue_->emplace(DNSPacketInfo(packet_info.domain_names, packet_info.response_code));
			}
		}
	}
}