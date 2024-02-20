#include "Detector.hpp"

using namespace std;
using namespace rigtorp;

std::atomic<bool> cancellation_token(false);

NetworkAnalyser* analyser_ptr;

#ifdef _WIN32
BOOL WINAPI console_handler(const DWORD signal)
{
	if (signal == CTRL_C_EVENT || signal == CTRL_BREAK_EVENT || signal == CTRL_CLOSE_EVENT)
	{
		cancellation_token.store(true);
		analyser_ptr->stop_capture();
		return TRUE;
	}
	return FALSE;
}
#else
#include <signal.h>
void signal_handler(int signum)
{
    cancellation_token.store(true);
    analyser_ptr->stop_capture();
}
#endif

int main(const int argc, const char** argv)
{
#ifdef _WIN32
	if (!SetConsoleCtrlHandler(console_handler, TRUE))
	{
		std::cerr << "ERROR: Could not set control handler" << std::endl;
		return EXIT_FAILURE;
	}
#else
    signal(SIGINT, signal_handler);
    signal(SIGTERM, signal_handler);
#endif

	unique_ptr<MPMCQueue<Packet>> packet_queue;
	unique_ptr<NetworkAnalyser> analyser;
	unique_ptr<Filter> filter;

	try
	{
		const auto args = make_unique<arguments>();

		args->parse(argc, argv);

		printf("%ld\n", args->packet_queue_size);
		packet_queue = make_unique<MPMCQueue<Packet>>(args->packet_queue_size);

		analyser = make_unique<NetworkAnalyser>(args->interface_to_sniff, args->packet_buffer_size, packet_queue.get());

		analyser_ptr = analyser.get();

		filter = make_unique<Filter>(packet_queue.get());
	}
	catch (const DetectorException& e)
	{
		cerr << "Error: " << e.what() << '\n';
		return e.get_code();
	}
	catch (const bad_alloc& e)
	{
		cerr << "Error: " << e.what() << '\n';
		cerr << "The size you entered is too huge" << '\n';
		return EXIT_FAILURE;
	}
	catch (const exception& e)
	{
		cerr << "Error: " << e.what() << '\n';
		return EXIT_FAILURE;
	}

	thread capture_thread(&NetworkAnalyser::start_capture, analyser.get());
	thread filter_thread(&Filter::process_packet, filter.get());

	capture_thread.join();
	filter_thread.join();

	return EXIT_SUCCESS;
}
