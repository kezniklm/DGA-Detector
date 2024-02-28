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
    ConfigureOptions(options);

    const json appsettings = LoadAppSettings();

    try
    {
        const auto result = options.parse(argc, argv);

        ValidateAndSetOptions(result, appsettings, options);
    }
    catch (const ArgumentException)
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
    options.add_options()("i,interface", "Interface to analyze DNS responses", cxxopts::value<string>(), "<interface name>")("s,size", "Allowed memory usage", cxxopts::value<unsigned long long>(), "<size in bytes>")("d,database", "Database connection string", cxxopts::value<string>(), "<connection string>")("r,rabbitmq", "RabbitMQ connection string", cxxopts::value<string>(), "<connection string>")("q,queue", "RabbitMQ queue name", cxxopts::value<string>(), "<queue name>")("h,help", "Show help");
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
            cerr << "Error parsing appsettings.json: " << e.what() << '\n';
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
        throw ArgumentException(options.help(), ARGUMENT_HELP);
    }

    try
    {
        SetOption<string>("interface", interface_to_sniff_, result, appsettings, true, options);
        SetOption<unsigned long long>("size", memory_size_, result, appsettings, true, options);
        CalculateSizes(memory_size_);
        SetOption<string>("database", database_connection_string_, result, appsettings, true, options);
        SetOption<string>("rabbitmq", rabbitmq_connection_string_, result, appsettings, true, options);
        SetOption<string>("queue", rabbitmq_queue_name_, result, appsettings, true, options);
    }
    catch (const std::exception &e)
    {
        cout << options.help() << '\n';
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
        cout << options.help() << '\n';
        throw ArgumentException("Missing required option: --" + key, ARGUMENT_CHECK_FAILURE);
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
    const unsigned long long divided_value = value / 16;
    packet_buffer_size_ =
        static_cast<int>(min(divided_value * 4, static_cast<unsigned long long>(numeric_limits<int>::max())));
    packet_queue_size_ = static_cast<int>(divided_value * 13 / sizeof(Packet));
    dns_info_queue_size_ = static_cast<int>(divided_value / sizeof(DNSPacketInfo));
    publisher_queue_size_ = 10000; // Default size
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
