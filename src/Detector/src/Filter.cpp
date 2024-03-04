/**
 * @file Filter.cpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Implements packet filtering functionality.
 *
 * The main functionalities of this file include:
 * - Defining the constructor for the Filter class.
 * - Processing packets by filtering DNS packets.
 * - Processing DNS packets to extract domain names and response codes.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 *
 */

#include "Filter.hpp"

using namespace std;

/**
 * @brief Constructs a Filter object with given packet and DNS queues.
 *
 * Initializes the Filter object with the provided packet queue and DNS info queue.
 *
 * @param packet_queue Pointer to the packet queue for incoming packets.
 * @param dns_queue Pointer to the DNS info queue for processed DNS packets.
 */
Filter::Filter(IQueue<Packet> *packet_queue, IQueue<DNSPacketInfo> *dns_queue) : packet_queue_(packet_queue), dns_info_queue_(dns_queue)
{
}

/**
 * @brief Processes packets from the packet queue.
 *
 * Processes packets by extracting DNS packets and passing them to ProcessDnsPacket function.
 * Continuously runs until cancellation token is set.
 */
void Filter::ProcessPacket() const
{
    struct Packet packet
    {
    };
    while (!cancellation_token.load())
    {
        if (packet_queue_->try_pop(packet))
        {
            ProcessDnsPacket(packet);
        }
        else
        {
            this_thread::sleep_for(chrono::milliseconds(100)); // Add sleep to reduce CPU usage
        }
    }
}

/**
 * @brief Processes DNS packets to extract domain names and response codes.
 *
 * Extracts domain names and response codes from DNS packets and adds them to the DNS info queue.
 *
 * @param custom_packet The custom packet structure containing packet data.
 */
void Filter::ProcessDnsPacket(const Packet &custom_packet) const
{
    constexpr int kQuery = 0;

    pcpp::RawPacket raw_packet(custom_packet.data, custom_packet.header.caplen, custom_packet.header.ts, false, pcpp::LINKTYPE_ETHERNET);
    const pcpp::Packet parsed_packet(&raw_packet);

    const auto *dns_layer = parsed_packet.getLayerOfType<pcpp::DnsLayer>();
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

    const int kResponseCode = dns_layer->getDnsHeader()->responseCode;

    dns_info_queue_->emplace(DNSPacketInfo(domain_names, kResponseCode));
}
