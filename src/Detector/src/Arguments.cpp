/**
 * @file Arguments.cpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Handles parsing and storing of command line arguments for the application.
 *
 * This file contains the implementation of the Arguments class, which is responsible for parsing command-line arguments and loading application settings. It utilizes the cxxopts library for parsing command-line arguments and interacts with JSON file (appsettings.json) to load application settings.
 *
 * The main functionalities of this file include:
 * - Parsing command-line arguments using cxxopts library.
 * - Loading application settings from a JSON file named "appsettings.json".
 * - Validating and setting options based on the provided arguments and application settings.
 * - Calculating buffer and queue sizes based on input values.
 * - Converting all keys in the loaded JSON settings to lowercase for uniform access.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 *
 */

#include "Arguments.hpp"

using namespace std;
using json = nlohmann::json;

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
void Arguments::Parse(const int argc, const char *argv[])
{
    cxxopts::Options options("Detector", "Detector of DNS responses in the DGA Detector system");

    try
    {
        ConfigureOptions(options);

        const json appsettings = LoadAppSettings();

        const auto result = options.parse(argc, argv);

        ValidateAndSetOptions(result, appsettings, options);
    }
    catch (ArgumentException)
    {
        cout << options.help() << '\n';
        throw;
    }
    catch (const exception &e)
    {
        cout << options.help() << '\n';
        throw ArgumentException(e.what(), ARGUMENT_CHECK_FAILURE);
    }
}

/**
 * Configures command line options for the program using cxxopts library.
 *
 * @param options Reference to the cxxopts::Options object to which the options will be added.
 *
 * @returns None
 */
void Arguments::ConfigureOptions(cxxopts::Options &options)
{
    options.add_options()("i,interface", "Interface to analyze DNS responses", cxxopts::value<string>(), "<interface name>")("s,size", "Allowed memory usage", cxxopts::value<unsigned long long>(), "<size in bytes>")("d,database", "Database connection string", cxxopts::value<string>(), "<connection string>")("r,rabbitmq", "RabbitMQ connection string", cxxopts::value<string>(), "<connection string>")("q,queue", "RabbitMQ queue name", cxxopts::value<string>(), "<queue name>")("t,threads", "Number of processing threads", cxxopts::value<int>(), "<number>")("b,max-batch-size", "Maximal size of batch to query the database", cxxopts::value<size_t>()->default_value("100000"), "<max batch size>")
        ("c,max-cycle-count", "Maximal number of cycles after which the database will be queried", cxxopts::value<size_t>()->default_value("50000"), "<max cycle count>")("h,help", "Show help");
}

/**
 * Loads application settings from a JSON file named "appsettings.json".
 *
 * @return A JSON object containing the loaded application settings with keys converted to lowercase.
 */
json Arguments::LoadAppSettings()
{
    ifstream appsettings_file("appsettings.json");
    json appsettings;

    if (appsettings_file.is_open())
    {
        try
        {
            appsettings_file >> appsettings;
        }
        catch (const json::parse_error &e)
        {
            throw ArgumentException(std::string("Error parsing appsettings.json: ") + e.what() + "\n", ARGUMENT_CHECK_FAILURE);
        }
        appsettings_file.close();
    }

    return MakeKeysLowercase(appsettings);
}

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
void Arguments::ValidateAndSetOptions(const cxxopts::ParseResult &result,
                                      const json &appsettings,
                                      const cxxopts::Options &options)
{
    if (result.count("help"))
    {
        throw ArgumentException("", ARGUMENT_HELP);
    }

    try
    {
        SetOption<string>("interface", interface_to_sniff_, result, appsettings, true, options);
        SetOption<unsigned long long>("size", memory_size_, result, appsettings, true, options);
        CalculateSizes(memory_size_);
        SetOption<string>("database", database_connection_string_, result, appsettings, true, options);
        SetOption<string>("rabbitmq", rabbitmq_connection_string_, result, appsettings, true, options);
        SetOption<string>("queue", rabbitmq_queue_name_, result, appsettings, true, options);
        SetOption<int>("threads", number_of_threads_, result, appsettings, false, options);
        SetOption<size_t>("max-batch-size", max_batch_size_, result, appsettings, false, options);
        SetOption<size_t>("max-cycle-count", max_cycle_count_, result, appsettings, false, options);

        if (max_batch_size_ == 0 || max_cycle_count_ == 0)
        {
            max_batch_size_ = 100000;
            max_cycle_count_ = 50000;
        }

        constexpr int DEFAULT_NUMBER_OF_THREADS = 5;
        if (number_of_threads_ < DEFAULT_NUMBER_OF_THREADS)
        {
            number_of_threads_ = std::thread::hardware_concurrency();
            if (number_of_threads_ == 0)
            {
                number_of_threads_ = DEFAULT_NUMBER_OF_THREADS;
            }
        }
    }
    catch (const std::exception &e)
    {
        throw ArgumentException(e.what(), ARGUMENT_CHECK_FAILURE);
    }
}

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
void Arguments::SetOption(const string &key,
                          T &value,
                          const cxxopts::ParseResult &result,
                          const json &appsettings,
                          const bool required,
                          const cxxopts::Options &options)
{
    if (result.count(key))
    {
        value = result[key].as<T>();
    }
    else if (appsettings.contains(key))
    {
        value = appsettings[key].get<T>();
    }
    else if (required)
    {
        throw ArgumentException("Missing required option: --" + key, ARGUMENT_CHECK_FAILURE);
    }

    // Trim quotes for string values
    if constexpr (std::is_same_v<T, std::string>)
    {
        value = TrimQuotes(value);
    }
}

/**
 * Calculates sizes of buffers and queues based on the input value.
 *
 * @param value The input value used for calculations.
 *
 * @returns None
 */
void Arguments::CalculateSizes(const unsigned long long value)
{
    const unsigned long long packet_buffer_size_alloc = std::min(value * 65 / 100, static_cast<unsigned long long>(std::numeric_limits<int>::max()));

    constexpr unsigned long long publisher_queue_memory = 1000 * sizeof(ValidatedDomains);

    const unsigned long long remaining_value = value - packet_buffer_size_alloc - publisher_queue_memory;

    constexpr double packet_queue_percentage = 35.0;

    const unsigned long long packet_queue_size_alloc = static_cast<unsigned long long>(packet_queue_percentage / 100.0 *
                                                                                       remaining_value);
    const unsigned long long dns_info_queue_size_alloc = remaining_value - packet_queue_size_alloc;

    packet_buffer_size_ = static_cast<int>(packet_buffer_size_alloc);
    packet_queue_size_ = static_cast<int>(packet_queue_size_alloc / sizeof(DetectorPacket));
    dns_info_queue_size_ = static_cast<int>(dns_info_queue_size_alloc / sizeof(DNSPacketInfo));
    publisher_queue_size_ = 1000; // Fixed size
}

/**
 * Creates a new JSON object with all keys converted to lowercase.
 *
 * @param original The original JSON object.
 *
 * @returns A new JSON object with lowercase keys.
 */
json Arguments::MakeKeysLowercase(const json &original)
{
    json lowercased;
    for (auto &[key, value] : original.items())
    {
        string lower_key = key;
        transform(lower_key.begin(), lower_key.end(), lower_key.begin(), ::tolower);
        lowercased[lower_key] = value;
    }
    return lowercased;
}

/**
 * Removes surrounding quotes from a string, if present.
 * @param input The input string potentially enclosed in quotes.
 * @returns The string without surrounding quotes.
 */
std::string Arguments::TrimQuotes(const std::string &input)
{
    if (input.length() >= 2 && (input.front() == '\"' && input.back() == '\"') || (input.front() == '\'' && input.back() == '\''))
    {
        return input.substr(1, input.length() - 2);
    }
    return input;
}

/**
 * Sets an option of type std::string in the Arguments object.
 *
 * @param key The key corresponding to the option.
 * @param value The value of the option to be set.
 * @param result The parse result containing the parsed options.
 * @param appsettings The JSON object containing application settings.
 * @param required A flag indicating if the option is required.
 * @param options The cxxopts::Options object containing available options.
 *
 * @returns None
 */
template void Arguments::SetOption<std::string>(const std::string &key,
                                                std::string &value,
                                                const cxxopts::ParseResult &result,
                                                const json &appsettings,
                                                bool required,
                                                const cxxopts::Options &options);

/**
 * Sets an option of type unsigned long long in the Arguments object.
 *
 * @param key The key corresponding to the option.
 * @param value Reference to the unsigned long long value to be set.
 * @param result The cxxopts::ParseResult object containing parsed options.
 * @param appsettings The json object containing application settings.
 * @param required Flag indicating if the option is required.
 * @param options The cxxopts::Options object containing available options.
 *
 * @returns None
 */
template void Arguments::SetOption<unsigned long long>(const std::string &key,
                                                       unsigned long long &value,
                                                       const cxxopts::ParseResult &result,
                                                       const json &appsettings,
                                                       bool required,
                                                       const cxxopts::Options &options);

/**
 * Gets the network interface name for sniffing.
 * @return The interface name.
 */
std::string Arguments::GetInterfaceToSniff() const
{
    return this->interface_to_sniff_;
}

/**
 * Gets the allocated memory size for processing.
 * @return The memory size in bytes.
 */
unsigned long long Arguments::GetMemorySize() const
{
    return this->memory_size_;
}

/**
 * Gets the database connection string.
 * @return The connection string for the database.
 */
std::string Arguments::GetDatabaseConnectionString() const
{
    return this->database_connection_string_;
}

/**
 * Gets the RabbitMQ connection string.
 * @return The connection string for RabbitMQ.
 */
std::string Arguments::GetRabbitMQConnectionString() const
{
    return this->rabbitmq_connection_string_;
}

/**
 * Gets the RabbitMQ queue name.
 * @return The name of the RabbitMQ queue.
 */
std::string Arguments::GetRabbitMQQueueName() const
{
    return this->rabbitmq_queue_name_;
}

/**
 * Gets the packet buffer size.
 * @return The size of the packet buffer.
 */
int Arguments::GetPacketBufferSize() const
{
    return this->packet_buffer_size_;
}

/**
 * Gets the packet queue size.
 * @return The size of the packet queue.
 */
size_t Arguments::GetPacketQueueSize() const
{
    return this->packet_queue_size_;
}

/**
 * Gets the DNS information queue size.
 * @return The size of the DNS information queue.
 */
size_t Arguments::GetDNSInfoQueueSize() const
{
    return this->dns_info_queue_size_;
}

/**
 * Gets the publisher queue size.
 * @return The size of the publisher queue.
 */
size_t Arguments::GetPublisherQueueSize() const
{
    return this->publisher_queue_size_;
}

/**
 * Sets the network interface name to be used for sniffing.
 * @param value The interface name.
 */
void Arguments::SetInterfaceToSniff(const std::string &value)
{
    this->interface_to_sniff_ = value;
}

/**
 * Sets the memory size allocated for processing.
 * @param value The memory size in bytes.
 */
void Arguments::SetMemorySize(const unsigned long long value)
{
    this->memory_size_ = value;
}

/**
 * Sets the database connection string.
 * @param value The connection string for the database.
 */
void Arguments::SetDatabaseConnectionString(const std::string &value)
{
    this->database_connection_string_ = value;
}

/**
 * Sets the RabbitMQ connection string.
 * @param value The connection string for RabbitMQ.
 */
void Arguments::SetRabbitMQConnectionString(const std::string &value)
{
    this->rabbitmq_connection_string_ = value;
}

/**
 * Sets the RabbitMQ queue name.
 * @param value The name of the RabbitMQ queue.
 */
void Arguments::SetRabbitMQQueueName(const std::string &value)
{
    this->rabbitmq_queue_name_ = value;
}

/**
 * Sets the packet buffer size.
 * @param value The size of the packet buffer.
 */
void Arguments::SetPacketBufferSize(const int value)
{
    this->packet_buffer_size_ = value;
}

/**
 * Sets the packet queue size.
 * @param value The size of the packet queue.
 */
void Arguments::SetPacketQueueSize(const size_t value)
{
    this->packet_queue_size_ = value;
}

/**
 * Sets the DNS information queue size.
 * @param value The size of the DNS information queue.
 */
void Arguments::SetDNSInfoQueueSize(const size_t value)
{
    this->dns_info_queue_size_ = value;
}

/**
 * Sets the publisher queue size.
 * @param value The size of the publisher queue.
 */
void Arguments::SetPublisherQueueSize(const size_t value)
{
    this->publisher_queue_size_ = value;
}

/**
 * Gets the current number of threads to be used for processing.
 * @return The current number of threads set for processing tasks.
 */
int Arguments::GetNumberOfThreads() const
{
    return this->number_of_threads_;
}

/**
 * Sets the number of threads to be used for processing.
 * Adjusting the number of threads can optimize the application's performance based on the hardware capabilities and the workload.
 * It's important to select a value that balances the workload distribution without overwhelming the system resources.
 * @param value The desired number of threads for processing tasks.
 */
void Arguments::SetNumberOfThreads(int value)
{
    this->number_of_threads_ = value;
}

/**
 * Gets the maximal size of batch to query the database.
 * @return The maximal batch size.
 */
size_t Arguments::GetMaxBatchSize() const
{
    return this->max_batch_size_;
}

/**
 * Sets the maximal size of batch to query the database.
 * @param value The maximal batch size.
 */
void Arguments::SetMaxBatchSize(const size_t value)
{
    this->max_batch_size_ = value;
}

/**
 * Gets the maximal number of cycles after which the database will be queried.
 * @return The maximal cycle count.
 */
size_t Arguments::GetMaxCycleCount() const
{
    return this->max_cycle_count_;
}

/**
 * Sets the maximal number of cycles after which the database will be queried.
 * @param value The maximal cycle count.
 */
void Arguments::SetMaxCycleCount(const size_t value)
{
    this->max_cycle_count_ = value;
}
