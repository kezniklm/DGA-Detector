#include "Detector.hpp"

using namespace std;

int main(const int argc, const char **argv)
{
	unique_ptr<NetworkAnalyser> analyser;

	try
	{
		const unique_ptr<arguments> args = make_unique<arguments>();

		args->parse(argc, argv);

		analyser = make_unique<NetworkAnalyser>(args->interface_to_sniff, args->packet_buffer_size);
	}
	catch (const exception& e)
	{
		cerr << "Error: " << e.what() << '\n';
	}

	analyser->start_capture();

	return EXIT_SUCCESS;
}
