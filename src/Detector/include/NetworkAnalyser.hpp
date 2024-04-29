/**
 * @file NetworkAnalyser.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Declares the NetworkAnalyser class for capturing and analyzing network traffic.
 *
 * The main functionalities of this class include:
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
 *
 */

#pragma once

#include <atomic>
#include <iostream>
#include <pcap.h>

#include "DetectorPacket.hpp"
#include "IQueue.hpp"
#include "NetworkAnalyserException.hpp"
#include "ReturnCodes.hpp"

/** Declare external cancellation token */
extern std::atomic<bool> cancellation_token;

/**
 * @brief NetworkAnalyser class for capturing and analyzing network traffic.
 */
class NetworkAnalyser
{
public:
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
    explicit NetworkAnalyser(const std::string &device,
                             int buffer_size,
                             IQueue<DetectorPacket> *packet_queue);

    /**
     * @brief Destroys the NetworkAnalyser object.
     *
     * Closes the pcap handle and prints statistics if available.
     */
    ~NetworkAnalyser();

    /**
     * @brief Starts capturing network traffic.
     *
     * Invokes pcap_loop to capture packets indefinitely.
     */
    void StartCapture() const;

    /**
     * @brief Stops capturing network traffic.
     *
     * Invokes pcap_breakloop to terminate pcap_loop.
     */
    void StopCapture() const;

    /**
     * @brief Getter for the pcap handle.
     *
     * Provides read-only access to the internal pcap handle used for capturing packets.
     * @return The pcap handle.
     */
    pcap_t *GetHandle() const;

    /**
     * @brief Getter for the packet queue pointer.
     *
     * Provides read-only access to the packet queue used for storing captured packets.
     * @return The pointer to the packet queue.
     */
    IQueue<DetectorPacket> *GetPacketQueue() const;

private:
    /** Pcap handle for packet capture */
    pcap_t *handle_;

    /** Pointer to the packet queue */
    IQueue<DetectorPacket> *queue_;

    /** Allows access from NetworkAnalyserTests class */
    friend class NetworkAnalyserTests;

    /**
     * @brief Creates a pcap handle for capturing network traffic.
     *
     * @param device Name of the network device to capture traffic from.
     */
    void CreateHandle(const char *device);

    /**
     * @brief Sets the buffer size for packet capture.
     *
     * @param buffer_size Size of the buffer.
     */
    void SetBufferSize(int buffer_size) const;

    /**
     * @brief Activates the pcap handle for packet capture.
     */
    void ActivateHandle() const;

    /**
     * @brief Sets immediate mode for packet capture.
     *
     * Immediate mode delivers packets to the application as soon as they are available.
     */
    void SetImmediateMode() const;

    /**
     * @brief Compiles and installs a Berkeley Packet Filter (BPF) expression for packet filtering.
     */
    void SetBpfFilter() const;

    /**
     * @brief Sets the snaplen for packet capture.
     *
     * Snaplen is the number of bytes to capture from each packet.
     */
    void SetSnaplen() const;

    /**
     * @brief Sets promiscuous mode for packet capture.
     *
     * Promiscuous mode allows the capture of all packets on the network, not just those destined for the device.
     */
    void SetPromiscuousMode() const;

    /**
     * @brief Sets the timeout for packet capture.
     *
     * @param timeout Timeout value in milliseconds.
     */
    void SetTimeout() const;

    /**
     * @brief Packet handler function called by pcap_loop.
     *
     * Enqueues the received packet into the packet queue.
     *
     * @param user User-defined pointer (pointer to the packet queue).
     * @param header Packet header.
     * @param packet Packet data.
     */
    static void PacketHandler(u_char *user, const struct pcap_pkthdr *header, const u_char *packet);
};
