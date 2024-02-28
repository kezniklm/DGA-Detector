#include "Detector.hpp"

using namespace std;
using namespace rigtorp;

std::atomic<bool> cancellation_token(false);

NetworkAnalyser *analyser_ptr;

#ifdef _WIN32
BOOL WINAPI console_handler(const DWORD signal)
{
	if (signal == CTRL_C_EVENT || signal == CTRL_BREAK_EVENT || signal == CTRL_CLOSE_EVENT)
	{
		cancellation_token.store(true);
		analyser_ptr->StopCapture();
		return TRUE;
	}
	return FALSE;
}
#else
#include <csignal>
void SignalHandler(int signum)
{
	cancellation_token.store(true);
	analyser_ptr->StopCapture();
}
#endif

int main(const int argc, const char **argv)
{
#ifdef _WIN32
	if (!SetConsoleCtrlHandler(console_handler, TRUE))
	{
		std::cerr << "ERROR: Could not set control handler" << std::endl;
		return EXIT_FAILURE;
	}
#else
	signal(SIGINT, SignalHandler);
	signal(SIGTERM, SignalHandler);
#endif

	unique_ptr<MPMCQueue<Packet>> packet_queue;
	unique_ptr<MPMCQueue<DNSPacketInfo>> dns_info_queue;
	unique_ptr<MPMCQueue<ValidatedDomains>> publisher_queue;
	unique_ptr<NetworkAnalyser> analyser;
	unique_ptr<Filter> filter;
	unique_ptr<MongoDBDatabase> database;
	unique_ptr<DomainValidator> validator;
	unique_ptr<MessagePublisher> rmq_agent; // change

	unique_ptr<Publisher> publisher; // change

	try
	{
		printf("DO NOT INTERRUPT PROGRAM NOW\n");
		unique_ptr<Arguments> args = make_unique<Arguments>();

		args->Parse(argc, argv);
		printf("%d\n", args->packet_buffer_size_);
		printf("%ld\n", args->packet_queue_size_);
		packet_queue = make_unique<MPMCQueue<Packet>>(args->packet_queue_size_);
		printf("%ld\n", args->dns_info_queue_size_);
		dns_info_queue = make_unique<MPMCQueue<DNSPacketInfo>>(args->dns_info_queue_size_);
		printf("%ld\n", args->publisher_queue_size_);
		publisher_queue = make_unique<MPMCQueue<ValidatedDomains>>(args->publisher_queue_size_);

		printf("YOU ARE NOW FREE TO DO EVERYTHING\n");

		analyser = make_unique<NetworkAnalyser>(args->interface_to_sniff_, args->packet_buffer_size_, packet_queue.get());

		analyser_ptr = analyser.get();

		filter = make_unique<Filter>(packet_queue.get(), dns_info_queue.get());

		database = make_unique<MongoDBDatabase>(args->database_connection_string_, "Database");

		validator = make_unique<DomainValidator>(dns_info_queue.get(), publisher_queue.get(), database.get());

		rmq_agent = make_unique<MessagePublisher>(args->rabbitmq_connection_string_, args->rabbitmq_queue_name_);

		publisher = make_unique<Publisher>(publisher_queue.get(), rmq_agent.get());
	}
	catch (const ArgumentException &e)
	{
		if (e.GetCode() != ARGUMENT_HELP)
		{
			cerr << "Error: " << e.what() << '\n';
		}
		return e.GetCode();
	}
	catch (const DetectorException &e)
	{
		cerr << "Error: " << e.what() << '\n';
		return e.GetCode();
	}
	catch (const bad_alloc &e)
	{
		cerr << "Error: " << e.what() << '\n';
		cerr << "The size you entered is too huge" << '\n';
		return EXIT_FAILURE;
	}
	catch (const exception &e)
	{
		cerr << "Error: " << e.what() << '\n';
		return EXIT_FAILURE;
	}

	thread capture_thread(&NetworkAnalyser::StartCapture, analyser.get());

	thread filter_thread(&Filter::ProcessPacket, filter.get());

	thread domain_validator_thread(&DomainValidator::ProcessDomains, validator.get());

	thread publisher_thread(&Publisher::Process, publisher.get());

	capture_thread.join();
	filter_thread.join();
	domain_validator_thread.join();
	publisher_thread.join();

	return EXIT_SUCCESS;
}
