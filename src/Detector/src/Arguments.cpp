#include "Arguments.hpp"

using namespace std;

void arguments::parse(const int argc, const char* argv[])
{
	cxxopts::Options options("Detector", "Detector of DNS responses in the DGA Detector system");

	options.add_options()("i,interface", "Interface", cxxopts::value<std::string>(),
	                      "Interface, where the Detector would be analysing the DNS responses")(
		"s,size", "Packet buffer size", cxxopts::value<unsigned long long>(),
		"Packet buffer size - choose carefully, maximum is INT32_MAX value")("h,help", "");

	try
	{
		const cxxopts::ParseResult result = options.parse(argc, argv);

		if (result.count("help"))
		{
			std::cout << options.help() << '\n';
			throw ArgumentException("", ARGUMENT_CHECK_FAILURE );
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
			
			if (value > static_cast<unsigned long long>(std::numeric_limits<int>::max()))
			{
				packet_buffer_size = numeric_limits<int>::max();
			}
			else
			{
				packet_buffer_size = static_cast<int>(value);
			}
		}
		else
		{
			std::cerr << options.help({""}) << '\n';
			throw ArgumentException("--size option is required.", ARGUMENT_CHECK_FAILURE);
		}
	}
	catch (const std::exception& e)
	{
		throw ArgumentException(e.what(), ARGUMENT_CHECK_FAILURE);
	}
}

void arguments::check_rabbit_mq_connection(const std::string& rabbitMqConnectionString)
{
}
