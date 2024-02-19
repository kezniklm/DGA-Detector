#include "Detector.hpp"

using namespace std;

int main(const int argc, const char** argv)
{
	unique_ptr<arguments> args = make_unique<arguments>();

	ResultCode arg_parse_result = args->parse(argc, argv);

	if(arg_parse_result.isFailure())
	{
		return EXIT_FAILURE;
	}

	unique_ptr<NetworkAnalyser> analyser = make_unique<NetworkAnalyser>(args->Interface);


	return EXIT_SUCCESS;
}
