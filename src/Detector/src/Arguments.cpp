#include "Arguments.hpp"

using namespace std;
using json = nlohmann::json;

void Arguments::Parse(const int argc, const char *argv[])
{
	cxxopts::Options options("Detector", "Detector of DNS responses in the DGA Detector system");
	ConfigureOptions(options);

	json appsettings = LoadAppSettings();

	try
	{
		auto result = options.parse(argc, argv);

		ValidateAndSetOptions(result, appsettings, options);
	}
	catch (const ArgumentException &e)
	{
		cout << options.help() << '\n';
		throw; // Re-throw to allow further handling if needed
	}
	catch (const exception &e)
	{
		cout << options.help() << '\n';
		throw ArgumentException(e.what(), ARGUMENT_CHECK_FAILURE);
	}
}

void Arguments::ConfigureOptions(cxxopts::Options& options) 
{
    options.add_options()
        ("i,interface", "Interface to analyze DNS responses", cxxopts::value<string>(), "<interface name>")
        ("s,size", "Allowed memory usage", cxxopts::value<unsigned long long>(), "<size in bytes>")
        ("d,database", "Database connection string", cxxopts::value<string>(), "<connection string>")
        ("r,rabbitmq", "RabbitMQ connection string", cxxopts::value<string>(), "<connection string>")
        ("q,queue", "RabbitMQ queue name", cxxopts::value<string>(), "<queue name>")
        ("h,help", "Show help");
}

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

void Arguments::ValidateAndSetOptions(const cxxopts::ParseResult &result, const json &appsettings, const cxxopts::Options &options)
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

template <typename T>
void Arguments::SetOption(const string &key, T &value, const cxxopts::ParseResult &result, const json &appsettings, bool required, const cxxopts::Options &options)
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

void Arguments::CalculateSizes(const unsigned long long value)
{
	unsigned long long divided_value = value / 16;
	packet_buffer_size_ = static_cast<int>(min(divided_value * 4, static_cast<unsigned long long>(numeric_limits<int>::max())));
	packet_queue_size_ = static_cast<int>(divided_value * 13 / sizeof(Packet));
	dns_info_queue_size_ = static_cast<int>(divided_value / sizeof(DNSPacketInfo));
	publisher_queue_size_ = 10000; // Default size
}

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

// Explicit template instantiation for known types
template void Arguments::SetOption<std::string>(const std::string &key, std::string &value, const cxxopts::ParseResult &result, const json &appsettings, bool required, const cxxopts::Options &options);
template void Arguments::SetOption<unsigned long long>(const std::string &key, unsigned long long &value, const cxxopts::ParseResult &result, const json &appsettings, bool required, const cxxopts::Options &options);
