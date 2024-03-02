/**
 * @file Arguments.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Declaration of classes and functions for handling command line arguments.
 *
 * This header file declares the Arguments class, which provides functionalities for parsing and storing command line arguments used by the application. It defines various member functions to parse command line arguments, load application settings, validate options, and calculate sizes of buffers and queues based on input values.
 *
 * The main components of this file include:
 * - Declaration of the Arguments class, which encapsulates the logic for handling command line arguments.
 * - Declaration of member variables to store command line options such as interface, memory size, database connection string, RabbitMQ connection string, and queue name.
 * - Declaration of member functions for parsing command line arguments, loading application settings, configuring options, and setting option values.
 * - Declaration of helper functions for calculating buffer and queue sizes and converting keys in loaded JSON settings to lowercase.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 *
 */

#pragma once

#include <algorithm>
#include <fstream>
#include <iostream>
#include <limits>

#include "cxxopts.hpp"
#include "nlohmann/json.hpp"

#include "ArgumentException.hpp"
#include "DNSPacketInfo.hpp"
#include "Packet.hpp"
#include "ReturnCodes.hpp"

#ifdef _WIN32
#undef max
#endif

/**
 * A class to handle parsing and storing command line arguments.
 */
class Arguments
{
public:
    /**
     * Parses the command line arguments and loads application settings.
     *
     * @param argc The number of command line arguments.
     * @param argv An array of C-strings containing the command line arguments.
     *
     * @throws ArgumentException if there is an issue with the arguments.
     *
     * @returns None
     */
    void Parse(const int argc, const char *argv[]);

    /**
     * Gets the network interface name for sniffing.
     * @return The interface name.
     */
    std::string GetInterfaceToSniff() const;

    /**
     * Gets the allocated memory size for processing.
     * @return The memory size in bytes.
     */
    unsigned long long GetMemorySize() const;

    /**
     * Gets the database connection string.
     * @return The connection string for the database.
     */
    std::string GetDatabaseConnectionString() const;

    /**
     * Gets the RabbitMQ connection string.
     * @return The connection string for RabbitMQ.
     */
    std::string GetRabbitMQConnectionString() const;

    /**
     * Gets the RabbitMQ queue name.
     * @return The name of the RabbitMQ queue.
     */
    std::string GetRabbitMQQueueName() const;

    /**
     * Gets the packet buffer size.
     * @return The size of the packet buffer.
     */
    int GetPacketBufferSize() const;

    /**
     * Gets the packet queue size.
     * @return The size of the packet queue.
     */
    size_t GetPacketQueueSize() const;

    /**
     * Gets the DNS information queue size.
     * @return The size of the DNS information queue.
     */
    size_t GetDNSInfoQueueSize() const;

    /**
     * Gets the publisher queue size.
     * @return The size of the publisher queue.
     */
    size_t GetPublisherQueueSize() const;

    /**
     * Sets the network interface name to be used for sniffing.
     * @param value The interface name.
     */
    void SetInterfaceToSniff(const std::string &value);

    /**
     * Sets the memory size allocated for processing.
     * @param value The memory size in bytes.
     */
    void SetMemorySize(const unsigned long long value);

    /**
     * Sets the database connection string.
     * @param value The connection string for the database.
     */
    void SetDatabaseConnectionString(const std::string &value);

    /**
     * Sets the RabbitMQ connection string.
     * @param value The connection string for RabbitMQ.
     */
    void SetRabbitMQConnectionString(const std::string &value);

    /**
     * Sets the RabbitMQ queue name.
     * @param value The name of the RabbitMQ queue.
     */
    void SetRabbitMQQueueName(const std::string &value);

    /**
     * Sets the packet buffer size.
     * @param value The size of the packet buffer.
     */
    void SetPacketBufferSize(const int value);

    /**
     * Sets the packet queue size.
     * @param value The size of the packet queue.
     */
    void SetPacketQueueSize(const size_t value);

    /**
     * Sets the DNS information queue size.
     * @param value The size of the DNS information queue.
     */
    void SetDNSInfoQueueSize(const size_t value);

    /**
     * Sets the publisher queue size.
     * @param value The size of the publisher queue.
     */
    void SetPublisherQueueSize(const size_t value);

private:
    /**
     * Configures command line options for the program using cxxopts library.
     *
     * @param options Reference to the cxxopts::Options object to which the options will be added.
     *
     * @returns None
     */
    void ConfigureOptions(cxxopts::Options &options);

    /**
     * Loads application settings from a JSON file named "appsettings.json".
     *
     * @return A JSON object containing the loaded application settings with keys converted to lowercase.
     */
    nlohmann::json LoadAppSettings();

    /**
     * Validates and sets the options based on the provided arguments and application settings.
     *
     * @param result The parsed command-line arguments.
     * @param appsettings The application settings in JSON format.
     * @param options The options for the command-line parser.
     *
     * @throws ArgumentException if there is an issue with the arguments or settings.
     *
     * @returns None
     */
    void ValidateAndSetOptions(const cxxopts::ParseResult &result,
                               const nlohmann::json &appsettings,
                               const cxxopts::Options &options);

    /**
     * Sets the option value based on the provided key and conditions.
     *
     * @param key The key corresponding to the option.
     * @param value The reference to the variable where the option value will be stored.
     * @param result The parsed command-line arguments.
     * @param appsettings The JSON object containing application settings.
     * @param required Flag indicating if the option is required.
     * @param options The options object containing help information.
     *
     * @returns None
     *
     * @throws ArgumentException if a required option is missing.
     */
    template <typename T>
    void SetOption(const std::string &key,
                   T &value,
                   const cxxopts::ParseResult &result,
                   const nlohmann::json &appsettings,
                   bool required,
                   const cxxopts::Options &options);

    /**
     * Calculates sizes of buffers and queues based on the input value.
     *
     * @param value The input value used for calculations.
     *
     * @returns None
     */
    void CalculateSizes(const unsigned long long value);

    /**
     * Creates a new JSON object with all keys converted to lowercase.
     *
     * @param original The original JSON object.
     *
     * @returns A new JSON object with lowercase keys.
     */
    static nlohmann::json MakeKeysLowercase(const nlohmann::json &original);

    /**
     * Removes surrounding quotes from a string, if present.
     * @param input The input string potentially enclosed in quotes.
     * @returns The string without surrounding quotes.
     */
    static std::string TrimQuotes(const std::string &input);

    /**
     * A string variable representing the interface to sniff.
     */
    std::string interface_to_sniff_;

    /**
     * Represents the size of memory in bytes as an unsigned long long integer.
     */
    unsigned long long memory_size_;

    /**
     * A string variable to store the connection string for the database.
     */
    std::string database_connection_string_;

    /**
     * The connection string for RabbitMQ.
     */
    std::string rabbitmq_connection_string_;

    /**
     * The name of the RabbitMQ queue.
     */
    std::string rabbitmq_queue_name_;

    /**
     * Size of the packet buffer.
     */
    int packet_buffer_size_;

    /**
     * The size of the packet queue.
     */
    size_t packet_queue_size_;

    /**
     * The size of the DNS information queue.
     */
    size_t dns_info_queue_size_;

    /**
     * The size of the queue used by the publisher.
     */
    size_t publisher_queue_size_;
};
