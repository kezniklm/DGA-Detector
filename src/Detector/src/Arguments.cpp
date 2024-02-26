#include "Arguments.hpp"

using namespace std;

void Arguments::Parse(const int argc, const char *argv[])
{
	cxxopts::Options options("Detector", "Detector of DNS responses in the DGA Detector system");

	options.add_options()("i,interface",
						  "Interface where the Detector will analyze the DNS responses",
						  cxxopts::value<std::string>(),
						  "<interface name>")("s,size",
											  "Size of memory that you allow the program to use",
											  cxxopts::value<unsigned long long>(),
											  "<size in bytes>")("d,database_",
																 "Connection string to database_ (using appsettings.json is safer)",
																 cxxopts::value<std::string>(),
																 "<connection_ string>")("r,rabbitmq",
																						 "Connection string to RabbitMQ (using appsettings.json is safer)",
																						 cxxopts::value<std::string>(),
																						 "<connection_ string>")(
		"q,queue",
		"Name of the RabbitMQ queue",
		cxxopts::value<
			std::string>(),
		"<queue name>")(
		"h,help",
		"Show help");

	std::ifstream appsettings_file("appsettings.json");

	nlohmann::json appsettings;

	if (appsettings_file.is_open())
	{
		try
		{
			appsettings_file >> appsettings;
			appsettings_file.close();
		}
		catch (const nlohmann::json::parse_error &e)
		{
			std::cerr << "Error parsing appsettings.json: " << e.what() << '\n';
		}
	}

	appsettings = MakeKeysLowercase(appsettings);

	try
	{
		const cxxopts::ParseResult kResult = options.parse(argc, argv);

		if (kResult.count("help"))
		{
			std::cout << options.help() << '\n';
			throw ArgumentException("", ARGUMENT_CHECK_FAILURE);
		}

		if (kResult.count("interface"))
		{
			interface_to_sniff_ = kResult["interface"].as<std::string>();
		} else if (appsettings.contains("interface"))
		{
			interface_to_sniff_ = appsettings["interface"];
		} else
		{
			std::cerr << options.help({""}) << '\n';
			throw ArgumentException("--interface option is required.", ARGUMENT_CHECK_FAILURE);
		}

		if (kResult.count("size"))
		{
			unsigned long long size = kResult["size"].as<unsigned long long>();
			CalculateSizes(size);
		} else if (appsettings.contains("size"))
		{
			unsigned long long size = appsettings["size"];
			CalculateSizes(size);
		} else
		{
			std::cerr << options.help({""}) << '\n';
			throw ArgumentException("--size option is required.", ARGUMENT_CHECK_FAILURE);
		}

		if (kResult.count("database_"))
		{
			database_connection_string_ = kResult["database_"].as<std::string>();
		} else if (appsettings.contains("database_"))
		{
			database_connection_string_ = appsettings["database_"];
		} else
		{
			std::cerr << options.help({""}) << '\n';
			throw ArgumentException("--database_ option is required.", ARGUMENT_CHECK_FAILURE);
		}

		if (kResult.count("rabbitmq"))
		{
			rabbitmq_connection_string_ = kResult["rabbitmq"].as<std::string>();
		} else if (appsettings.contains("rabbitmq"))
		{
			rabbitmq_connection_string_ = appsettings["rabbitmq"];
		} else
		{
			std::cerr << options.help({""}) << '\n';
			throw ArgumentException("--rabbitmq option is required.", ARGUMENT_CHECK_FAILURE);
		}

		if (kResult.count("queue"))
		{
			rabbitmq_queue_name_ = kResult["queue"].as<std::string>();
		} else if (appsettings.contains("queue"))
		{
			rabbitmq_queue_name_ = appsettings["queue"];
		} else
		{
			std::cerr << options.help({""}) << '\n';
			throw ArgumentException("--rabbitmq queue name option is required.", ARGUMENT_CHECK_FAILURE);
		}
	}
	catch (const std::exception &e)
	{
		throw ArgumentException(e.what(), ARGUMENT_CHECK_FAILURE);
	}
}

void Arguments::CalculateSizes(const unsigned long long value)
{
	unsigned long long divided_value = value / 16;

	if (divided_value * 4 > static_cast<unsigned long long>(std::numeric_limits<int>::max()))
	{
		packet_buffer_size_ = numeric_limits<int>::max();
	} else
	{
		packet_buffer_size_ = static_cast<int>(divided_value * 4);
	}

	packet_queue_size_ = (divided_value * 13) / sizeof(struct Packet);

	dns_info_queue_size_ = divided_value / sizeof(struct DNSPacketInfo);

	publisher_queue_size_ = dns_info_queue_size_;
}

nlohmann::json Arguments::MakeKeysLowercase(const nlohmann::json &original)
{
	nlohmann::json lowercased;
	for (auto it = original.begin(); it != original.end(); ++it)
	{
		std::string key = it.key();
		std::transform(key.begin(), key.end(), key.begin(),
					   [](unsigned char c) { return std::tolower(c); });
		lowercased[key] = it.value();
	}
	return lowercased;
}
