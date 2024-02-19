#pragma once

#include <pcap.h>
#include <iostream>
#include <cstring>

class NetworkAnalyser
{
public:
    NetworkAnalyser(const std::string interface_param);
    void processPacket(const struct pcap_pkthdr *header, const u_char *packet);

private:
    const char *interface;
    char *device;
    pcap_t *handle_;

    static constexpr const char *filter_ = "port 53";
};

// class PacketCapture
// {
// public:
//     PacketCapture(const std::string &device) : device_(device), handle_(nullptr)
//     {
//         char errbuf[PCAP_ERRBUF_SIZE];

//         // Open the device for capturing
//         handle_ = pcap_open_live(device_.c_str(), 65535, 1, 1000, errbuf);
//         if (handle_ == nullptr)
//         {
//             throw std::runtime_error(std::string("Could not open device: ") + errbuf);
//         }

//         // Set buffer size for better performance
//         if (pcap_set_buffer_size(handle_, 1024 * 1024 * 100) != 0)
//         {
//             std::cerr << "Warning: Could not set buffer size: " << pcap_geterr(handle_) << std::endl;
//         }

// // Enable immediate mode, if supported
// #ifdef PCAP_IMMEDIATE_MODE
//         if (pcap_set_immediate_mode(handle_, 1) != 0)
//         {
//             std::cerr << "Warning: Could not set immediate mode: " << pcap_geterr(handle_) << std::endl;
//         }
// #endif
//     }

//     ~PacketCapture()
//     {
//         if (handle_ != nullptr)
//         {
//             pcap_close(handle_);
//         }
//     }

//     // Disallow copy and assignment
//     PacketCapture(const PacketCapture &) = delete;
//     PacketCapture &operator=(const PacketCapture &) = delete;

//     // Static callback function to adapt pcap_loop callback signature to member function
//     static void packetHandler(u_char *user, const struct pcap_pkthdr *header, const u_char *packet)
//     {
//         auto *obj = reinterpret_cast<PacketCapture *>(user);
//         obj->processPacket(header, packet);
//     }

//     void startCapture(int num_packets)
//     {
//         if (handle_ != nullptr)
//         {
//             pcap_loop(handle_, num_packets, packetHandler, reinterpret_cast<u_char *>(this));
//         }
//     }

//     void stopCapture()
//     {
//         if (handle_ != nullptr)
//         {
//             pcap_breakloop(handle_);
//         }
//     }

// protected:
//     void processPacket(const struct pcap_pkthdr *header, const u_char *packet)
//     {
//         // Implement your packet processing logic here
//         // For example:
//         std::cout << "Packet captured with length: " << header->len << std::endl;
//     }

// private:
//     std::string device_;
//     pcap_t *handle_;
// };

// int main()
// {
//     try
//     {
//         std::string device = "your_device_name"; // Replace with your device name
//         PacketCapture pcap(device);
//         pcap.startCapture(0); // Pass 0 for infinite loop
//     }
//     catch (const std::runtime_error &e)
//     {
//         std::cerr << "Error: " << e.what() << std::endl;
//     }
//     return 0;
// }
