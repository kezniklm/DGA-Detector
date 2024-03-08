/**
 * @file Detector.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Declaration of the Detector class for monitoring and analyzing network traffic.
 *
 * This header file declares the Detector class, which is responsible for orchestrating the monitoring of network traffic, filtering packets, validating domains, and publishing messages. It also includes declarations for signal handling setup and external variables used for cancellation and global network analyser pointer.
 *
 * The main functionalities of this file include:
 * - Declaration of the Detector class with its constructor and member functions.
 * - Declaration of signal handling functions.
 * - Declaration of external variables for cancellation and global network analyser pointer.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 *
 */

#pragma once

#include <csignal>
#include <iostream>
#include <memory>
#include <thread>
#include <unordered_set>

#include "Arguments.hpp"
#include "DNSPacketInfo.hpp"
#include "DomainValidator.hpp"
#include "DetectorPacket.hpp"
#include "Filter.hpp"
#include "IDatabase.hpp"
#include "Logger.hpp"
#include "MessagePublisher.hpp"
#include "MPMCQueueWrapper.hpp"
#include "MongoDbDatabase.hpp"
#include "NetworkAnalyser.hpp"
#include "Publisher.hpp"

/** Pointer to the global NetworkAnalyser instance. */
extern NetworkAnalyser *global_analyser_ptr;

/** Declare external logger pointer */
extern Logger *global_logger_ptr;

/**
 * @brief The Detector class orchestrates the monitoring of network traffic, filtering packets, validating domains, and publishing messages.
 */
class Detector
{
public:
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
    Detector(const int argc, const char **argv);

    /**
     * @brief Starts monitoring network traffic and processing packets.
     *
     * This function starts the monitoring of network traffic by launching multiple threads for capturing packets, filtering packets, validating domains, and publishing messages.
     *
     * @returns None
     */
    void Run();

    /**
     * @brief Setter method for the network analyser.
     * @param analyser Pointer to the NetworkAnalyser object.
     */
    void SetAnalyser(std::unique_ptr<NetworkAnalyser> analyser);

    /**
     * @brief Setter method for the message publisher.
     * @param publisher Pointer to the Publisher object.
     */
    void SetPublisher(std::unique_ptr<Publisher> publisher);

    /**
     * @brief Setter method for the domain validator.
     * @param validator Pointer to the DomainValidator object.
     */
    void SetValidator(std::unique_ptr<DomainValidator> validator);

    /**
     * @brief Getter method for the network analyser.
     * @return Pointer to the NetworkAnalyser object.
     */
    NetworkAnalyser *GetAnalyser() const;

    /**
     * @brief Getter method for the message publisher.
     * @return Pointer to the Publisher object.
     */
    Publisher *GetPublisher() const;

    /**
     * @brief Getter method for the domain validator.
     * @return Pointer to the DomainValidator object.
     */
    DomainValidator *GetValidator() const;

    /**
     * Gets the number of processing threads.
     * @return The number of threads.
     */
    unsigned int GetNumberOfThreads() const;

    /**
     * Sets the number of processing threads.
     * @param value The number of threads.
     */
    void SetNumberOfThreads(unsigned int value);

private:
    /**
     * @brief Initializes the components required for network traffic monitoring.
     *
     * Initializes the components required for network traffic monitoring, including parsing command-line arguments, setting up queues, network analyser, filter, database, domain validator, and message publisher.
     *
     * @param argc The number of command-line arguments.
     * @param argv An array of C-style strings containing the command-line arguments.
     * @throws ArgumentException if there is an issue with the arguments.
     * @throws DetectorException if there is an error during initialization.
     * @throws std::bad_alloc if memory allocation fails.
     */
    void InitializeComponents(const int argc, const char **argv);

    /**
     * @brief Signal handler for SIGINT and SIGTERM signals.
     *
     * This function handles SIGINT and SIGTERM signals by setting the cancellation token to true and stopping the network capture if a global network analyser pointer is set.
     *
     * @param signum The signal number.
     * @returns None
     */
    static void SignalHandler(int signum);

    /**
     * @brief Sets up signal handling for graceful termination of the program.
     *
     * This function sets up signal handlers to capture signals such as SIGINT and SIGTERM for terminating the program gracefully. On Windows, it uses SetConsoleCtrlHandler, while on Unix-like systems, it uses signal() to register signal handlers.
     *
     * @returns None
     */
    static void SetupSignalHandling();

    /** Queue for packets */
    std::unique_ptr<IQueue<DetectorPacket>> packet_queue_;

    /** Queue for DNS packet information */
    std::unique_ptr<IQueue<DNSPacketInfo>> dns_info_queue_;

    /** Queue for validated domains */
    std::unique_ptr<IQueue<ValidatedDomains>> publisher_queue_;

    /** Network analyser instance */
    std::unique_ptr<NetworkAnalyser> analyser_;

    /** Packet filter instance */
    std::unique_ptr<Filter> filter_;

    /** MongoDB database instance */
    std::unique_ptr<MongoDbDatabase> database_;

    /** Domain validator instance */
    std::unique_ptr<DomainValidator> validator_;

    /** Message publisher instance */
    std::unique_ptr<MessagePublisher> message_publisher_;

    /** Publisher instance */
    std::unique_ptr<Publisher> publisher_;

    /** Logger instance */
    std::unique_ptr<Logger> logger_;

    /** Number of threads used by the Filter */
    int number_of_threads_;
};
