#pragma once

#include <cstring>
#include <pcap.h>

constexpr size_t kMaxPacketSize = 2048; // Define a maximum packet size

struct Packet
{
	struct pcap_pkthdr header;
	u_char data[kMaxPacketSize]{}; // Fixed-size array on the stack

	Packet() = default;

	Packet(const struct pcap_pkthdr &hdr, const u_char *packet_data) : header(hdr)
	{
		// Ensure we don't overflow the fixed-size buffer
		const size_t kCopySize = hdr.len < kMaxPacketSize ? hdr.len : kMaxPacketSize;
		memcpy(data, packet_data, kCopySize);
	}
};
