/**
 * @file NetworkAnalyser.cpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Implements the NetworkAnalyser class for capturing and analyzing network traffic.
 *
 * The main functionalities of this file include:
 * - Creating and configuring a pcap handle for network traffic capture.
 * - Starting and stopping the packet capture process.
 * - Handling received packets and enqueuing them into a thread-safe queue.
 * - Setting up signal handling for graceful termination of packet capture.
 * - Calculating buffer sizes, snaplen, timeout, and activating the pcap handle.
 * - Compiling and installing Berkeley Packet Filter (BPF) expressions for filtering packets.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 */

#include "NetworkAnalyser.hpp"

#ifndef PCAP_NETMASK_UNKNOWN
#define PCAP_NETMASK_UNKNOWN 0xffffffff
#endif

using namespace std;

/**
 * @brief Constructs a NetworkAnalyser object.
 *
 * Initializes the pcap handle, sets up buffer size, snaplen, promiscuous mode, timeout,
 * activates the handle, sets immediate mode if available, and sets BPF filter.
 *
 * @param device The name of the network device to capture traffic from.
 * @param buffer_size The buffer size for packet capture.
 * @param packet_queue Pointer to the packet queue for enqueuing captured packets.
 */
NetworkAnalyser::NetworkAnalyser(const string &device, const int buffer_size,
                                 IQueue<DetectorPacket> *packet_queue)
    : handle_(nullptr), queue_(packet_queue)
{
    CreateHandle(device.c_str());

    SetBufferSize(buffer_size);

    SetSnaplen();

    SetPromiscuousMode();

    SetTimeout();

    ActivateHandle();

#ifdef PCAP_IMMEDIATE_MODE
    SetImmediateMode();
#endif

    SetBpfFilter();
}

/**
 * @brief Destroys the NetworkAnalyser object.
 *
 * Closes the pcap handle and prints statistics if available.
 */
NetworkAnalyser::~NetworkAnalyser()
{
    struct pcap_stat stats
    {
    };
    if (pcap_stats(handle_, &stats) == 0)
    {
        printf("Number of packets received: %u\n", stats.ps_recv);
        printf("Number of packets dropped: %u\n", stats.ps_drop);
        printf("Number of packets dropped by the interface or operating system: %u\n", stats.ps_ifdrop);
    }
    else
    {
        fprintf(stderr, "Error getting pcap stats: %s\n", pcap_geterr(handle_));
    }

    if (handle_ != nullptr)
    {
        pcap_close(handle_);
        handle_ = nullptr;
    }
}

/**
 * @brief Packet handler function called by pcap_loop.
 *
 * Enqueues the received packet into the packet queue.
 *
 * @param user User-defined pointer (pointer to the packet queue).
 * @param header Packet header.
 * @param packet Packet data.
 */
void NetworkAnalyser::PacketHandler(u_char *user, const struct pcap_pkthdr *header, const u_char *packet)
{
    auto *queue = reinterpret_cast<IQueue<DetectorPacket> *>(user);
    queue->emplace(DetectorPacket(*header, packet));
}

/**
 * @brief Starts capturing network traffic.
 *
 * Invokes pcap_loop to capture packets indefinitely.
 */
void NetworkAnalyser::StartCapture() const
{
    if (handle_ != nullptr)
    {
        pcap_loop(handle_, -1, PacketHandler, reinterpret_cast<u_char *>(queue_));
    }
}

/**
 * @brief Stops capturing network traffic.
 *
 * Invokes pcap_breakloop to terminate pcap_loop.
 */
void NetworkAnalyser::StopCapture() const
{
    if (handle_ != nullptr)
    {
        pcap_breakloop(handle_);
    }
}

/**
 * @brief Creates a pcap handle for capturing network traffic.
 *
 * @param device Name of the network device to capture traffic from.
 */
void NetworkAnalyser::CreateHandle(const char *device)
{
    char errbuf[PCAP_ERRBUF_SIZE];
    handle_ = pcap_create(device, errbuf);
    if (handle_ == nullptr)
    {
        throw NetworkAnalyserException(string("Could not open device: ") + errbuf, NETWORK_ANALYSER_CREATION_FAILURE);
    }
}

/**
 * @brief Sets the buffer size for packet capture.
 *
 * @param buffer_size Size of the buffer.
 */
void NetworkAnalyser::SetBufferSize(int buffer_size) const
{
    constexpr int MIN_BUFFER_SIZE = 1024 * 1024;
    constexpr int DECREMENT_SIZE = 1024 * 1024 * 5;

    while (buffer_size >= MIN_BUFFER_SIZE)
    {
        if (pcap_set_buffer_size(handle_, buffer_size) == 0)
        {
            return;
        }
        buffer_size -= DECREMENT_SIZE;
    }

    throw NetworkAnalyserException("Buffer size could not have been set", NETWORK_ANALYSER_CREATION_FAILURE);
}

/**
 * @brief Sets the snaplen for packet capture.
 *
 * Snaplen is the number of bytes to capture from each packet.
 */
void NetworkAnalyser::SetSnaplen() const
{
    constexpr int PCAP_SNAPLEN = 65535;
    if (pcap_set_snaplen(handle_, PCAP_SNAPLEN) == PCAP_ERROR_ACTIVATED)
    {
        throw NetworkAnalyserException("The operation pcap_set_snaplen can't be performed on already activated captures",
                                       NETWORK_ANALYSER_CREATION_FAILURE);
    }
}

/**
 * @brief Sets promiscuous mode for packet capture.
 *
 * Promiscuous mode allows the capture of all packets on the network, not just those destined for the device.
 */
void NetworkAnalyser::SetPromiscuousMode() const
{
    constexpr int PROMISC_MODE = 1;
    if (pcap_set_promisc(handle_, PROMISC_MODE) == PCAP_ERROR_ACTIVATED)
    {
        throw NetworkAnalyserException("The operation pcap_set_promisc() can't be performed on already activated captures",
                                       NETWORK_ANALYSER_CREATION_FAILURE);
    }
}

/**
 * @brief Sets the timeout for packet capture.
 *
 * @param timeout Timeout value in milliseconds.
 */
void NetworkAnalyser::SetTimeout() const
{
    constexpr int TIMEOUT_IN_MS = 1;
    if (pcap_set_timeout(handle_, TIMEOUT_IN_MS) == PCAP_ERROR_ACTIVATED)
    {
        throw NetworkAnalyserException("The operation pcap_set_timeout() can't be performed on already activated captures",
                                       NETWORK_ANALYSER_CREATION_FAILURE);
    }
}

/**
 * @brief Activates the pcap handle for packet capture.
 */
void NetworkAnalyser::ActivateHandle() const
{
    constexpr int OK = 0;
    if (pcap_activate(handle_) != OK)
    {
        throw NetworkAnalyserException(string("Could not activate pcap handle: ") + pcap_geterr(handle_),
                                       NETWORK_ANALYSER_CREATION_FAILURE);
    }
}

#ifdef PCAP_IMMEDIATE_MODE
/**
 * @brief Sets immediate mode for packet capture.
 *
 * Immediate mode delivers packets to the application as soon as they are available.
 */
void NetworkAnalyser::SetImmediateMode() const
{
    constexpr int IMMEDIATE_MODE = 1;
    if (pcap_set_immediate_mode(handle_, IMMEDIATE_MODE) == PCAP_ERROR_ACTIVATED)
    {
        throw NetworkAnalyserException(
            "The operation pcap_set_immediate_mode() can't be performed on already activated captures",
            NETWORK_ANALYSER_CREATION_FAILURE);
    }
}
#endif

/**
 * @brief Compiles and installs a Berkeley Packet Filter (BPF) expression for packet filtering.
 */
void NetworkAnalyser::SetBpfFilter() const
{
    bpf_program fp{};

    constexpr char DNS_FILTER_EXPRESSION[] = "port 53";

    if (pcap_compile(handle_, &fp, DNS_FILTER_EXPRESSION, 0, PCAP_NETMASK_UNKNOWN) == -1)
    {
        throw NetworkAnalyserException(string("Could not Parse filter: ") + pcap_geterr(handle_),
                                       NETWORK_ANALYSER_CREATION_FAILURE);
    }

    if (pcap_setfilter(handle_, &fp) == -1)
    {
        throw NetworkAnalyserException(string("Could not install filter: ") + pcap_geterr(handle_),
                                       NETWORK_ANALYSER_CREATION_FAILURE);
    }

    pcap_freecode(&fp);
}

/**
 * @brief Getter for the pcap handle.
 *
 * Provides read-only access to the internal pcap handle used for capturing packets.
 * @return The pcap handle.
 */
pcap_t *NetworkAnalyser::GetHandle() const { return handle_; }

/**
 * @brief Getter for the packet queue pointer.
 *
 * Provides read-only access to the packet queue used for storing captured packets.
 * @return The pointer to the packet queue.
 */
IQueue<DetectorPacket> *NetworkAnalyser::GetPacketQueue() const { return queue_; }
