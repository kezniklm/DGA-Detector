#include "NetworkAnalyser.hpp"

#include <pcap.h>

NetworkAnalyser::NetworkAnalyser(const std::string& device, int buffer_size): handle_(nullptr)
{
	try
	{
		try_to_create_handle(device.c_str());

		try_to_set_buffer_size(buffer_size);

		try_to_activate_handle();

		// Enable immediate mode, if supported
#ifdef PCAP_IMMEDIATE_MODE
            try_to_set_immediate_mode();
#endif
		try_to_set_bpf_filter();
	}

	catch (const std::runtime_error& e)
	{
		std::cerr << "Error: " << e.what() << std::endl;
	}
}

NetworkAnalyser::~NetworkAnalyser()
{
	if (handle_ != nullptr)
	{
		pcap_close(handle_);
	}
}

// Static callback function to adapt pcap_loop callback signature to member function
static void packet_handler(u_char* user, const struct pcap_pkthdr* header, const u_char* packet)
{
	printf("Velkost: %d\n", header->len);
}

void NetworkAnalyser::start_capture() const
{
	if (handle_ != nullptr)
	{
		pcap_loop(handle_, -1, packet_handler, nullptr /*reinterpret_cast<u_char *>(this)*/);
	}
}

void NetworkAnalyser::stop_capture() const
{
	if (handle_ != nullptr)
	{
		pcap_breakloop(handle_);
	}
}

void NetworkAnalyser::try_to_create_handle(const char* device)
{
	char errbuf[PCAP_ERRBUF_SIZE];
	handle_ = pcap_create(device, errbuf);
	if (handle_ == nullptr)
	{
		throw std::runtime_error(std::string("Could not open device: ") + errbuf);
	}
}

void NetworkAnalyser::try_to_set_buffer_size(int buffer_size) const
{
	constexpr uint32_t min_buffer_size = 1024 * 1024; // Minimum buffer size to try is 1 MB
	constexpr uint32_t decrement_size = 1024 * 1024 * 5; // Decrease by 5 MB on each unsuccessful attempt

	while (buffer_size >= min_buffer_size)
	{
		if (pcap_set_buffer_size(handle_, buffer_size) == 0)
		{
			std::cout << "Max buffer size set to: " << buffer_size << " bytes" << std::endl;
			return; // Successfully set the buffer size, exit the function
		}
		// Decrease the buffer size for the next attempt
		buffer_size -= decrement_size;
	}

	std::cerr << "Failed to set buffer size to any value above " << min_buffer_size << " bytes." << std::endl;
}

void NetworkAnalyser::try_to_activate_handle() const
{
	// Activate the capture
	if (pcap_activate(handle_) != 0)
	{
		throw std::runtime_error(std::string("Could not activate pcap handle: ") + pcap_geterr(handle_));
	}
}

void NetworkAnalyser::try_to_set_immediate_mode() const
{
	if (pcap_set_immediate_mode(handle_, 1) != 0)
	{
		std::cerr << "Warning: Could not set immediate mode: " << pcap_geterr(handle_) << std::endl;
	}
}

void NetworkAnalyser::try_to_set_bpf_filter() const
{
	// Set BPF filter to "port 53"
	bpf_program fp;
	constexpr char filter_exp[] = "port 53"; // BPF filter
	if (pcap_compile(handle_, &fp, filter_exp, 0, PCAP_NETMASK_UNKNOWN) == -1)
	{
		throw std::runtime_error(std::string("Could not parse filter: ") + pcap_geterr(handle_));
	}
	if (pcap_setfilter(handle_, &fp) == -1)
	{
		throw std::runtime_error(std::string("Could not install filter: ") + pcap_geterr(handle_));
	}
	pcap_freecode(&fp); // Free the compiled filter
}
