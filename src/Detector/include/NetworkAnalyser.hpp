#pragma once

#include <pcap.h>
#include <iostream>

class NetworkAnalyser
{
public:
	NetworkAnalyser(const std::string& device, int buffer_size);

	~NetworkAnalyser();

	void start_capture() const;

	void stop_capture() const;

private:
	pcap_t* handle_;

	inline void try_to_create_handle(const char* device);

	inline void try_to_set_buffer_size(int buffer_size) const;

	inline void try_to_activate_handle() const;

	inline void try_to_set_immediate_mode() const;

	inline void try_to_set_bpf_filter() const;
};
