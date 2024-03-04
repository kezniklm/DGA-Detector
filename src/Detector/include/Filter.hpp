/**
 * @file Filter.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Declares the Filter class for packet filtering.
 *
 * This header file declares the Filter class, which is responsible for packet filtering functionality.
 * It includes necessary header files and defines the class interface.
 *
 * The main functionalities of this class include:
 * - Processing packets from the packet queue.
 * - Extracting DNS packets and passing them to ProcessDnsPacket function.
 * - Extracting domain names and response codes from DNS packets.
 *
 * @version 1.0
 * @date 2024-02-28
 */

#pragma once

#include <atomic>
#include <thread>

#include "DnsLayer.h"
#include "DNSPacketInfo.hpp"
#include "IPv4Layer.h"
#include "Packet.h"
#include "PayloadLayer.h"
#include "PcapPlusPlusVersion.h"
#include "ProtocolType.h"
#include "UdpLayer.h"

#include "IQueue.hpp"
#include "Packet.hpp"

/** Declare external cancellation token */
extern std::atomic<bool> cancellation_token;

/**
 * @brief Represents the Filter class for packet filtering.
 *
 * The Filter class is responsible for processing packets and filtering out DNS packets.
 * It provides methods for processing packets and extracting DNS information.
 */
class Filter
{
public:
    /**
     * @brief Constructs a Filter object with given packet and DNS queues.
     *
     * Initializes the Filter object with the provided packet queue and DNS info queue.
     *
     * @param packet_queue Pointer to the packet queue for incoming packets.
     * @param dns_queue Pointer to the DNS info queue for processed DNS packets.
     */
    explicit Filter(IQueue<Packet> *packet_queue, IQueue<DNSPacketInfo> *dns_queue);

    /**
     * @brief Processes packets from the packet queue.
     *
     * Processes packets by extracting DNS packets and passing them to ProcessDnsPacket function.
     * Continuously runs until cancellation token is set.
     */
    void ProcessPacket() const;

private:
    /** Pointer to the packet queue */
    IQueue<Packet> *packet_queue_;

    /** Pointer to the DNS info queue */
    IQueue<DNSPacketInfo> *dns_info_queue_;

    /**
     * @brief Processes DNS packets to extract domain names and response codes.
     *
     * Extracts domain names and response codes from DNS packets and adds them to the DNS info queue.
     *
     * @param custom_packet The custom packet structure containing packet data.
     */
    void ProcessDnsPacket(const Packet &custom_packet) const;
};
