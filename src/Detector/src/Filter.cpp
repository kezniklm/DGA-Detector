#include "Filter.hpp"

using namespace std;

Filter::Filter(rigtorp::MPMCQueue<Packet> *packet_queue, rigtorp::MPMCQueue<DNSPacketInfo> *dns_queue) : packet_queue_(
	packet_queue), dns_info_queue_(dns_queue)
{
}

void Filter::ProcessPacket()
{
	struct Packet packet{};
	while (!cancellation_token.load())
	{
		if (packet_queue_->try_pop(packet))
		{
			ProcessDnsPacket(packet);
		}
	}
}

void Filter::ProcessDnsPacket(const Packet &custom_packet)
{
	constexpr int kQuery = 0;

	pcpp::RawPacket raw_packet
		(custom_packet.data, custom_packet.header.caplen, custom_packet.header.ts, false, pcpp::LINKTYPE_ETHERNET);
	pcpp::Packet parsed_packet(&raw_packet);

	auto *dns_layer = parsed_packet.getLayerOfType<pcpp::DnsLayer>();
	if (dns_layer == nullptr)
	{
		return; // Failed to Parse DNS layer
	}

	if (dns_layer->getDnsHeader()->queryOrResponse == kQuery)
	{
		return;
	}

	std::vector<std::string> domain_names;
	for (pcpp::DnsQuery *query = dns_layer->getFirstQuery(); query != nullptr; query = dns_layer->getNextQuery(query))
	{
		domain_names.push_back(query->getName());
	}

	const int response_code = dns_layer->getDnsHeader()->responseCode;

	dns_info_queue_->emplace(DNSPacketInfo(domain_names, response_code));
}
