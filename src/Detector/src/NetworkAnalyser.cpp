#include "NetworkAnalyser.hpp"

#include <pcap.h>

using namespace std;

NetworkAnalyser::NetworkAnalyser(const string &device, int buffer_size) : handle_(nullptr)
{
	try
	{
		try_to_create_handle(device.c_str());

		try_to_set_buffer_size(buffer_size);

		set_snaplen();

		set_promiscuous_mode();

		set_timeout();

		try_to_activate_handle();

// Enable immediate mode, if supported
#ifdef PCAP_IMMEDIATE_MODE
		try_to_set_immediate_mode();
#endif
		try_to_set_bpf_filter();
	}

	catch (const runtime_error &e)
	{
		cerr << "Error: " << e.what() << endl;
		if (handle_ != nullptr)
		{
			pcap_close(handle_);
			handle_ = nullptr;
		}
		throw;
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
static void packet_handler(u_char *user, const struct pcap_pkthdr *header, const u_char *packet)
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

void NetworkAnalyser::try_to_create_handle(const char *device)
{
	char errbuf[PCAP_ERRBUF_SIZE];
	handle_ = pcap_create(device, errbuf);
	if (handle_ == nullptr)
	{
		throw runtime_error(string("Could not open device: ") + errbuf);
	}
}

void NetworkAnalyser::try_to_set_buffer_size(int buffer_size) const
{
	constexpr uint32_t min_buffer_size = 1024 * 1024;	 // Minimum buffer size to try is 1 MB
	constexpr uint32_t decrement_size = 1024 * 1024 * 5; // Decrease by 5 MB on each unsuccessful attempt

	while (buffer_size >= min_buffer_size)
	{
		if (pcap_set_buffer_size(handle_, buffer_size) == 0)
		{
			return;
		}
		buffer_size -= decrement_size;
	}

	throw runtime_error("Buffer size could not have been set");
}

void NetworkAnalyser::set_snaplen() const
{
	constexpr int PCAP_SNAPLEN = 65535;
	if (pcap_set_snaplen(handle_, PCAP_SNAPLEN) == PCAP_ERROR_ACTIVATED)
	{
		throw runtime_error("The operation pcap_set_snaplen can't be performed on already activated captures");
	}
}

void NetworkAnalyser::set_promiscuous_mode() const
{
	constexpr int PROMISC_MODE = 1;
	if (pcap_set_promisc(handle_, PROMISC_MODE) == PCAP_ERROR_ACTIVATED)
	{
		throw runtime_error("The operation pcap_set_promisc() can't be performed on already activated captures");
	}
}

void NetworkAnalyser::set_timeout() const
{
	constexpr int timeout_in_ms = 1;
	if (pcap_set_timeout(handle_, timeout_in_ms) == PCAP_ERROR_ACTIVATED)
	{
		throw runtime_error("The operation pcap_set_timeout() can't be performed on already activated captures");
	}
}

void NetworkAnalyser::try_to_activate_handle() const
{
	constexpr int OK = 0;
	if (pcap_activate(handle_) != OK)
	{
		throw runtime_error(string("Could not activate pcap handle: ") + pcap_geterr(handle_));
	}
}

void NetworkAnalyser::try_to_set_immediate_mode() const
{
	constexpr int IMMEDIATE_MODE = 1;
	if (pcap_set_immediate_mode(handle_, IMMEDIATE_MODE) == PCAP_ERROR_ACTIVATED)
	{
		throw runtime_error("The operation pcap_set_immediate_mode() can't be performed on already activated captures");
	}
}

void NetworkAnalyser::try_to_set_bpf_filter() const
{
	bpf_program fp;

	constexpr char dns_filter_exp[] = "port 53";

	if (pcap_compile(handle_, &fp, dns_filter_exp, 0, PCAP_NETMASK_UNKNOWN) == -1)
	{
		throw runtime_error(string("Could not parse filter: ") + pcap_geterr(handle_));
	}

	if (pcap_setfilter(handle_, &fp) == -1)
	{
		throw runtime_error(string("Could not install filter: ") + pcap_geterr(handle_));
	}

	pcap_freecode(&fp);
}
