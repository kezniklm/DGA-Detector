/**
 * @file Packet.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Declaration of the Packet struct for representing network packets.
 *
 * The main components of this file include:
 * - Declaration of the Packet struct, which represents network packets with a fixed-size buffer for packet data.
 * - Declaration of member variables to store packet header and data.
 * - Declaration of constructors for creating Packet objects with provided packet header and data.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 *
 */

#pragma once

#include <cstring>
#include <pcap.h>

/**
 * @struct Packet
 * @brief Represents a network packet.
 */
struct Packet
{
    /**
     * @brief Constructs a Packet object with packet header and data.
     *
     * @param hdr The packet header.
     * @param packet_data Pointer to the packet data.
     */
    Packet(const pcap_pkthdr &hdr, const u_char *packet_data) : header(hdr)
    {
        // Ensure we don't overflow the fixed-size buffer
        const size_t kCopySize = hdr.len < MAX_PACKET_SIZE ? hdr.len : MAX_PACKET_SIZE;
        memcpy(data, packet_data, kCopySize);
    }

    /**
     * @brief Default constructor.
     */
    Packet() = default;

    static constexpr size_t MAX_PACKET_SIZE = 2048; // Define a maximum packet size

    pcap_pkthdr header;
    u_char data[MAX_PACKET_SIZE]{};
};
