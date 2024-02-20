#pragma once

#include <iostream>
#include <pcap.h>

#include "MPMCQueue.hpp"

#include "Exceptions.hpp"
#include "Packet.hpp"
#include "ReturnCodes.hpp"

class NetworkAnalyser
{
public:
    explicit NetworkAnalyser(const std::string &device, const int buffer_size, rigtorp::MPMCQueue<Packet> *packet_queue);

    ~NetworkAnalyser();

    void start_capture() const;

    void stop_capture() const;

private:
    pcap_t *handle_;

    rigtorp::MPMCQueue<Packet> *queue_;

    void try_to_create_handle(const char *device);

    void try_to_set_buffer_size(int buffer_size) const;

    void try_to_activate_handle() const;

    void try_to_set_immediate_mode() const;

    void try_to_set_bpf_filter() const;

    void set_snaplen() const;

    void set_promiscuous_mode() const;

    void set_timeout() const;
};
