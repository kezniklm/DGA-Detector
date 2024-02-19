#include "Arguments.hpp"

ResultCode arguments::parse(const int argc, const char *argv[])
{
    cxxopts::Options options("Detector", "Detector of DNS responses in the DGA Detector system");

    options.add_options()("i,interface", "Interface", cxxopts::value<std::string>(), "Inteface, where the Detector would be analysing the DNS responses")("s,size","Packet buffer size",cxxopts::value<unsigned long long>(),"Packet buffer size - choose carefully, advised maximum is 85%% of device memory")("h,help", "");

    try
    {
        cxxopts::ParseResult result = options.parse(argc, argv);

        if (result.count("help"))
        {
            std::cout << options.help() << std::endl;
            return ResultCode(ResultCode::Code::Failure, "");
        }

        if (result.count("interface"))
        {
            interface_to_sniff = result["interface"].as<std::string>();
        }
        else
        {
            std::cerr << options.help({""}) << std::endl;
            return ResultCode(ResultCode::Code::Failure, "--interface option is required.");
        }

        if(result.count("size"))
        {
            unsigned long long size = result["size"].as<unsigned long long>(); //pretecenie
            if(size > INT32_MAX)
            {
                size = INT32_MAX;
            }
            packet_buffer_size = size;
        }
        else
        {
            std::cerr << options.help({""}) << std::endl;
            return ResultCode(ResultCode::Code::Failure, "--size option is required.");
        }

        return ResultCode(ResultCode::Code::Success, "");
    }
    catch (const std::exception &e)
    {
        return ResultCode(ResultCode::Code::Failure, std::string(e.what()));
    }
}

void arguments::check_rabbit_mq_connection(const std::string &rabbitMqConnectionString)
{
}
