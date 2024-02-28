#include "NetworkAnalyser.hpp"

using namespace std;
using namespace rigtorp;

NetworkAnalyser::NetworkAnalyser(const string &device, int buffer_size,
								 MPMCQueue<Packet> *packet_queue)
	: handle_(nullptr), queue_(packet_queue)
{
	TryToCreateHandle(device.c_str());

	TryToSetBufferSize(buffer_size);

	SetSnaplen();

	SetPromiscuousMode();

	SetTimeout();

	TryToActivateHandle();

#ifdef PCAP_IMMEDIATE_MODE
	TryToSetImmediateMode();
#endif

	TryToSetBpfFilter();
}

NetworkAnalyser::~NetworkAnalyser()
{
	struct pcap_stat stats;
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

static void PacketHandler(u_char *user, const struct pcap_pkthdr *header, const u_char *packet)
{
	auto *queue = reinterpret_cast<MPMCQueue<Packet> *>(user);
	queue->emplace(Packet(*header, packet));
}

void NetworkAnalyser::StartCapture() const
{
	if (handle_ != nullptr)
	{
		pcap_loop(handle_, -1, PacketHandler, reinterpret_cast<u_char *>(queue_));
	}
}

void NetworkAnalyser::StopCapture() const
{
	if (handle_ != nullptr)
	{
		pcap_breakloop(handle_);
	}
}

void NetworkAnalyser::TryToCreateHandle(const char *device)
{
	char errbuf[PCAP_ERRBUF_SIZE];
	handle_ = pcap_create(device, errbuf);
	if (handle_ == nullptr)
	{
		throw NetworkAnalyserException(string("Could not open device: ") + errbuf, NETWORK_ANALYSER_CREATION_FAILURE);
	}
}

void NetworkAnalyser::TryToSetBufferSize(int buffer_size) const
{
	constexpr uint32_t MIN_BUFFER_SIZE = 1024 * 1024;
	constexpr uint32_t DECREMENT_SIZE = 1024 * 1024 * 5;

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

void NetworkAnalyser::SetSnaplen() const
{
	constexpr int PCAP_SNAPLEN = 65535;
	if (pcap_set_snaplen(handle_, PCAP_SNAPLEN) == PCAP_ERROR_ACTIVATED)
	{
		throw NetworkAnalyserException("The operation pcap_set_snaplen can't be performed on already activated captures", NETWORK_ANALYSER_CREATION_FAILURE);
	}
}

void NetworkAnalyser::SetPromiscuousMode() const
{
	constexpr int PROMISC_MODE = 1;
	if (pcap_set_promisc(handle_, PROMISC_MODE) == PCAP_ERROR_ACTIVATED)
	{
		throw NetworkAnalyserException("The operation pcap_set_promisc() can't be performed on already activated captures", NETWORK_ANALYSER_CREATION_FAILURE);
	}
}

void NetworkAnalyser::SetTimeout() const
{
	constexpr int TIMEOUT_IN_MS = 1;
	if (pcap_set_timeout(handle_, TIMEOUT_IN_MS) == PCAP_ERROR_ACTIVATED)
	{
		throw NetworkAnalyserException("The operation pcap_set_timeout() can't be performed on already activated captures", NETWORK_ANALYSER_CREATION_FAILURE);
	}
}

void NetworkAnalyser::TryToActivateHandle() const
{
	constexpr int OK = 0;
	if (pcap_activate(handle_) != OK)
	{
		throw NetworkAnalyserException(string("Could not activate pcap handle: ") + pcap_geterr(handle_), NETWORK_ANALYSER_CREATION_FAILURE);
	}
}

void NetworkAnalyser::TryToSetImmediateMode() const
{
	constexpr int IMMEDIATE_MODE = 1;
	if (pcap_set_immediate_mode(handle_, IMMEDIATE_MODE) == PCAP_ERROR_ACTIVATED)
	{
		throw NetworkAnalyserException("The operation pcap_set_immediate_mode() can't be performed on already activated captures", NETWORK_ANALYSER_CREATION_FAILURE);
	}
}

void NetworkAnalyser::TryToSetBpfFilter() const
{
	bpf_program fp{};

	constexpr char DNS_FILTER_EXPRESSION[] = "port 53";

	if (pcap_compile(handle_, &fp, DNS_FILTER_EXPRESSION, 0, PCAP_NETMASK_UNKNOWN) == -1)
	{
		throw NetworkAnalyserException(string("Could not Parse filter: ") + pcap_geterr(handle_), NETWORK_ANALYSER_CREATION_FAILURE);
	}

	if (pcap_setfilter(handle_, &fp) == -1)
	{
		throw NetworkAnalyserException(string("Could not install filter: ") + pcap_geterr(handle_), NETWORK_ANALYSER_CREATION_FAILURE);
	}

	pcap_freecode(&fp);
}
