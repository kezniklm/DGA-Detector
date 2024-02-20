#pragma once

#include <pcap.h> // Make sure you have the pcap library included
#include <string.h>

constexpr size_t MAX_PACKET_SIZE = 2048; // Define a maximum packet size

struct Packet
{
	struct pcap_pkthdr header;
	u_char data[MAX_PACKET_SIZE]; // Fixed-size array on the stack

	Packet() = default;

	Packet(const struct pcap_pkthdr& hdr, const u_char* packet_data) : header(hdr)
	{
		// Ensure we don't overflow the fixed-size buffer
		const size_t copy_size = hdr.len < MAX_PACKET_SIZE ? hdr.len : MAX_PACKET_SIZE;
		memcpy(data, packet_data, copy_size);
	}
};
