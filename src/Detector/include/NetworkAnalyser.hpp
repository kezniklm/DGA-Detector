#pragma once

#include <pcap.h>
#include <iostream>

#include "Exceptions.hpp"
#include "ReturnCodes.hpp"

class NetworkAnalyser
{
public:
    explicit NetworkAnalyser(const std::string &device, int buffer_size);

    ~NetworkAnalyser();

    void start_capture() const;

    void stop_capture() const;

private:
    pcap_t *handle_;

    void try_to_create_handle(const char *device);

    void try_to_set_buffer_size(int buffer_size) const;

    void try_to_activate_handle() const;

    void try_to_set_immediate_mode() const;

    void try_to_set_bpf_filter() const;

    void set_snaplen() const;

    void set_promiscuous_mode() const;

    void set_timeout() const;
};
