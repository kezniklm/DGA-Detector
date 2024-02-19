#include "Arguments.hpp"

ResultCode arguments::parse(const int argc, const char *argv[])
{
    cxxopts::Options options("Detector", "Detector of DNS responses in the DGA Detector system");

    options.add_options()("i,interface", "Interface", cxxopts::value<std::string>(), "Inteface, where the Detector would be analysing the DNS responses")("h,help", "");

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
            Interface = result["interface"].as<std::string>();
            return ResultCode(ResultCode::Code::Success, "");
        }
        else
        {
            std::cerr << options.help({""}) << std::endl;
            return ResultCode(ResultCode::Code::Failure, "--interface option is required.");
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
