#include "Filter.hpp"

using namespace std;

Filter::Filter(rigtorp::MPMCQueue<Packet>* packet_queue) : queue_(packet_queue)
{
}

void Filter::process_packet() const
{
	struct Packet packet;
	while (!cancellation_token.load())
	{
		if (queue_->try_pop(packet))
		{
			// Assuming try_pop is the correct method to pop an item based on rigtorp MPMCQueue
			process_dns_packet(packet);
		}
	}
}

void Filter::process_dns_packet(const Packet& custom_packet)
{
	pcpp::RawPacket raw_packet(custom_packet.data, custom_packet.header.caplen, custom_packet.header.ts, false,
	                           pcpp::LINKTYPE_ETHERNET);

	const pcpp::Packet parsed_packet(&raw_packet);

	const pcpp::DnsLayer* dns_layer = parsed_packet.getLayerOfType<pcpp::DnsLayer>();
	printf("%d, %d\n", dns_layer != nullptr, dns_layer->getDnsHeader()->responseCode);
	if (dns_layer != nullptr && dns_layer->getDnsHeader()->queryOrResponse)
	{
		for (auto answer = dns_layer->getFirstAnswer(); answer != nullptr; answer = dns_layer->getNextAnswer(answer))
		{
			// Ensure you're comparing against the correct enum values
			if (answer->getDnsType() == pcpp::DNS_TYPE_A || answer->getDnsType() == pcpp::DNS_TYPE_AAAA)
			{
				std::string domainName = answer->getName();
				if (answer->getDataLength() == 0)
				{
					std::cout << "NXDOMAIN: " << domainName << std::endl;
				}
				else
				{
					std::cout << "Domain: " << domainName << " - Not NXDOMAIN" << std::endl; // TODO TCP asi nefunguje
				}
			}
		}
	}
}
