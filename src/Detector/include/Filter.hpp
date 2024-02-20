#pragma once

#include <iostream>
#include <memory>

#include "DnsLayer.h"
#include "IPv4Layer.h"
#include "MPMCQueue.hpp"
#include "Packet.h"
#include "Packet.hpp"
#include "PayloadLayer.h"
#include "PcapPlusPlusVersion.h"
#include "ProtocolType.h"
#include "UdpLayer.h"


extern std::atomic<bool> cancellation_token;

class Filter
{
public:
	explicit Filter(rigtorp::MPMCQueue<Packet>* packet_queue);

	void process_packet() const;

private:
	rigtorp::MPMCQueue<Packet>* queue_;

	static void process_dns_packet(const Packet& custom_packet);
};
