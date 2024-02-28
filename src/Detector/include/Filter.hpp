#pragma once

#include "DnsLayer.h"
#include "DNSPacketInfo.hpp"
#include "IPv4Layer.h"
#include "Packet.h"
#include "Packet.hpp"
#include "PayloadLayer.h"
#include "PcapPlusPlusVersion.h"
#include "ProtocolType.h"
#include "rigtorp/MPMCQueue.h"
#include "UdpLayer.h"

extern std::atomic<bool> cancellation_token;

class Filter
{
public:
	explicit Filter(rigtorp::MPMCQueue<Packet> *packet_queue, rigtorp::MPMCQueue<DNSPacketInfo> *dns_queue);

	void ProcessPacket();

private:
	rigtorp::MPMCQueue<Packet> *packet_queue_;
	rigtorp::MPMCQueue<DNSPacketInfo> *dns_info_queue_;

	void ProcessDnsPacket(const Packet &custom_packet);
};
