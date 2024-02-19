#include "NetworkAnalyser.hpp"

#include "pcap.h"

using namespace std;

NetworkAnalyser::NetworkAnalyser(const std::string interface_param)
{
    interface = interface_param.c_str();
    printf("%s", interface);


}

// void NetworkAnalyser::Capture()
// {
// }
