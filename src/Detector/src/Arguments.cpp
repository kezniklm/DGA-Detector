#include "Arguments.hpp"

using namespace std;

void arguments::parse(const int argc, const char *argv[])
{
	cxxopts::Options options("Detector", "Detector of DNS responses in the DGA Detector system");

	options.add_options()("i,interface", "Interface", cxxopts::value<std::string>(),
						  "Interface, where the Detector would be analysing the DNS responses")(
		"s,size", "Size", cxxopts::value<unsigned long long>(),
		"Size of memory that you allow program to use")("h,help", "");

	try
	{
		const cxxopts::ParseResult result = options.parse(argc, argv);

		if (result.count("help"))
		{
			std::cout << options.help() << '\n';
			throw ArgumentException("", ARGUMENT_CHECK_FAILURE);
		}

		if (result.count("interface"))
		{
			interface_to_sniff = result["interface"].as<std::string>();
		}
		else
		{
			std::cerr << options.help({""}) << '\n';
			throw ArgumentException("--interface option is required.", ARGUMENT_CHECK_FAILURE);
		}

		if (result.count("size"))
		{
			const unsigned long long value = result["size"].as<unsigned long long>();

			unsigned long long divided_value = value / 2;

			if (divided_value > static_cast<unsigned long long>(std::numeric_limits<int>::max()))
			{
				packet_buffer_size = numeric_limits<int>::max();
			}
			else
			{
				packet_buffer_size = static_cast<int>(value);
			}

			packet_queue_size = divided_value / sizeof(struct Packet);
		}
		else
		{
			std::cerr << options.help({""}) << '\n';
			throw ArgumentException("--size option is required.", ARGUMENT_CHECK_FAILURE);
		}
	}
	catch (const std::exception &e)
	{
		throw ArgumentException(e.what(), ARGUMENT_CHECK_FAILURE);
	}
}

void arguments::check_rabbit_mq_connection(const std::string &rabbitMqConnectionString)
{
}
