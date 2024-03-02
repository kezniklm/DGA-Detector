/**
 * @file Detector.cpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Implements the functionality for network traffic monitoring and analysis.
 *
 * This file contains the implementation of the Detector class, which orchestrates the monitoring of network traffic, filtering packets, validating domains, and publishing messages. It also includes signal handling setup for graceful termination.
 *
 * The main functionalities of this file include:
 * - Setting up signal handling for graceful termination of the program.
 * - Constructing a Detector object and initializing its components.
 * - Starting monitoring network traffic and processing packets.
 * - Initializing the components required for network traffic monitoring.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 *
 */

#include "Detector.hpp"

using namespace rigtorp;
using namespace std;

/** Atomic bool for cancellation token */
atomic<bool> cancellation_token(false);

/** Global pointer alowing to break the pcap loop */
NetworkAnalyser *global_analyser_ptr = nullptr;

/**
 * @brief Sets up signal handling for graceful termination of the program.
 *
 * This function sets up signal handlers to capture signals such as SIGINT and SIGTERM for terminating the program gracefully. On Windows, it uses SetConsoleCtrlHandler, while on Unix-like systems, it uses signal() to register signal handlers.
 *
 * @returns None
 */
void Detector::SetupSignalHandling()
{
#ifdef _WIN32
    // Windows signal handling
    SetConsoleCtrlHandler([](const DWORD signal) -> BOOL
                          {
        if (signal == CTRL_C_EVENT || signal == CTRL_BREAK_EVENT || signal == CTRL_CLOSE_EVENT) {
            cancellation_token.store(true);
            if (global_analyser_ptr) {
                global_analyser_ptr->StopCapture();
            }
            return TRUE;
        }
        return FALSE; },
                          TRUE);
#else
    // Unix-like signal handling
    signal(SIGINT, Detector::SignalHandler);
    signal(SIGTERM, Detector::SignalHandler);
#endif
}

/**
 * @brief Signal handler for SIGINT and SIGTERM signals.
 *
 * This function handles SIGINT and SIGTERM signals by setting the cancellation token to true and stopping the network capture if a global network analyser pointer is set.
 *
 * @param signum The signal number.
 * @returns None
 */
void Detector::SignalHandler(int signum)
{
    cancellation_token.store(true);
    if (global_analyser_ptr)
    {
        global_analyser_ptr->StopCapture();
    }
}

/**
 * @brief Constructs a Detector object and initializes its components.
 *
 * This constructor initializes signal handling, then initializes the components of the Detector class by parsing command-line arguments and setting up queues, network analyser, filter, database, domain validator, and message publisher.
 *
 * @param argc The number of command-line arguments.
 * @param argv An array of C-style strings containing the command-line arguments.
 * @throws ArgumentException if there is an issue with the arguments.
 * @throws DetectorException if there is an error during initialization.
 * @throws std::bad_alloc if memory allocation fails.
 * @returns None
 */
Detector::Detector(const int argc, const char **argv)
{
    SetupSignalHandling();
    InitializeComponents(argc, argv);
}

/**
 * @brief Starts monitoring network traffic and processing packets.
 *
 * This function starts the monitoring of network traffic by launching multiple threads for capturing packets, filtering packets, validating domains, and publishing messages.
 *
 * @returns None
 */
void Detector::Run()
{
    cout << "Monitoring network traffic. Press Ctrl+C to stop.\n";

    thread capture_thread(&NetworkAnalyser::StartCapture, analyser_.get());
    thread filter_thread(&Filter::ProcessPacket, filter_.get());
    thread domain_validator_thread(&DomainValidator::ProcessDomains, validator_.get());
    thread publisher_thread(&Publisher::Process, publisher_.get());

    capture_thread.join();
    filter_thread.join();
    domain_validator_thread.join();
    publisher_thread.join();
}

/**
 * @brief Initializes the components required for network traffic monitoring.
 *
 * This function initializes the components required for network traffic monitoring, including parsing command-line arguments, setting up queues, network analyser, filter, database, domain validator, and message publisher.
 *
 * @param argc The number of command-line arguments.
 * @param argv An array of C-style strings containing the command-line arguments.
 * @throws ArgumentException if there is an issue with the arguments.
 * @throws DetectorException if there is an error during initialization.
 * @throws std::bad_alloc if memory allocation fails.
 * @returns None
 */
void Detector::InitializeComponents(const int argc, const char **argv)
{
    try
    {
        cout << "Do not interrupt program now, interruption might cause leaks\n";
        const auto args = make_unique<Arguments>();
        args->Parse(argc, argv);

        packet_queue_ = make_unique<MPMCQueue<Packet>>(args->GetPacketQueueSize());
        dns_info_queue_ = make_unique<MPMCQueue<DNSPacketInfo>>(args->GetDNSInfoQueueSize());
        publisher_queue_ = make_unique<MPMCQueue<ValidatedDomains>>(args->GetPublisherQueueSize());

        analyser_ = make_unique<NetworkAnalyser>(args->GetInterfaceToSniff(), args->GetPacketBufferSize(), packet_queue_.get());
        global_analyser_ptr = analyser_.get();

        filter_ = make_unique<Filter>(packet_queue_.get(), dns_info_queue_.get());
        database_ = make_unique<MongoDbDatabase>(args->GetDatabaseConnectionString(), "Database");
        validator_ = make_unique<DomainValidator>(dns_info_queue_.get(), publisher_queue_.get(), database_.get());
        message_publisher_ = make_unique<MessagePublisher>(args->GetRabbitMQConnectionString(), args->GetRabbitMQQueueName());
        publisher_ = make_unique<Publisher>(publisher_queue_.get(), message_publisher_.get());
        cout << "You are now free to do everything\n";
    }
    catch (const ArgumentException &e)
    {
        throw;
    }
    catch (const DetectorException &e)
    {
        cerr << "Error: " << e.what() << '\n';
        throw;
    }
    catch (const bad_alloc &e)
    {
        cerr << "Error: " << e.what() << '\n';
        cerr << "The size you entered is too huge" << '\n';
        throw;
    }
    catch (const exception &e)
    {
        cerr << "Error: " << e.what() << '\n';
        throw;
    }
}
