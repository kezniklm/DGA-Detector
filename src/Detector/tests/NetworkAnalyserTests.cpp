/**
 * @file NetworkAnalyserTests.cpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Implements unit tests for the NetworkAnalyser class.
 *
 * This file contains the implementation of unit tests for the NetworkAnalyser class. The NetworkAnalyser class is responsible for capturing network packets from a specified network interface and processing them. These unit tests cover various scenarios to ensure the correct behavior of the NetworkAnalyser class under different conditions.
 *
 * The main functionalities of this file include:
 * - Testing the initialization of the NetworkAnalyser with a valid and invalid network device.
 * - Verifying proper handling of setting buffer size, snap length, promiscuous mode, and timeout.
 * - Ensuring proper initiation and termination of packet capture.
 * - Checking the installation of a Berkeley Packet Filter (BPF) filter.
 * - Verifying that network resources are released upon destruction of the NetworkAnalyser object.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 *
 */

#include "gtest/gtest.h"
#include "NetworkAnalyser.hpp"
#include "SharedMocks.hpp"
#include "pcap.h"
#include <thread>

using ::testing::_;
using ::testing::AtLeast;
using namespace std;

/**
 * @class NetworkAnalyserTests
 * @brief Test suite for NetworkAnalyser class.
 *
 * NetworkAnalyserTests is a test suite class derived from ::testing::Test provided by the Google Test framework.
 * It utilizes mock objects to simulate interactions with network interfaces and queues for storing captured packets.
 * The main goal is to test the NetworkAnalyser class's ability to capture and process network packets under various conditions without relying on real external dependencies.
 */
class NetworkAnalyserTests : public ::testing::Test
{
public:
    virtual void SetUp() override
    {
        std::optional<std::string> device_opt = FindSuitableDevice();
        if (!device_opt)
        {
            FAIL() << "No suitable network device found";
        }

        int buffer_size = 100 * 1024 * 1024;
        queue = new MockPacketQueue();
        analyser = new NetworkAnalyser(device_opt.value(), buffer_size, queue);
        analyser_private = new NetworkAnalyserPrivate(*analyser);

        handle = analyser->GetHandle();
    }

    virtual void TearDown() override
    {
        delete analyser;
        delete queue;
        
    }

private:
    /**
     * @struct NetworkAnalyserPrivate
     * @brief Private helper struct for accessing internal members of NetworkAnalyser.
     *
     * NetworkAnalyserPrivate provides access to private members and functions of the NetworkAnalyser class,
     * facilitating unit testing by allowing manipulation of internal state without exposing it publicly.
     */
    struct NetworkAnalyserPrivate
    {

        void (NetworkAnalyser::*CreateHandle)(const char *device);
        void (NetworkAnalyser::*SetBufferSize)(int buffer_size) const;
        void (NetworkAnalyser::*ActivateHandle)() const;
        void (NetworkAnalyser::*SetImmediateMode)() const;
        void (NetworkAnalyser::*SetBpfFilter)() const;
        void (NetworkAnalyser::*SetSnaplen)() const;
        void (NetworkAnalyser::*SetPromiscuousMode)() const;
        void (NetworkAnalyser::*SetTimeout)() const;
        static void (*PacketHandler)(u_char *user, const struct pcap_pkthdr *header, const u_char *packet);

        /**
         * @brief Constructs a NetworkAnalyserPrivate object.
         * @param analyser A reference to the NetworkAnalyser object.
         */
        NetworkAnalyserPrivate(NetworkAnalyser &analyser)
            : CreateHandle(&NetworkAnalyser::CreateHandle),
              SetBufferSize(&NetworkAnalyser::SetBufferSize),
              ActivateHandle(&NetworkAnalyser::ActivateHandle),
              SetBpfFilter(&NetworkAnalyser::SetBpfFilter),
              SetSnaplen(&NetworkAnalyser::SetSnaplen),
              SetPromiscuousMode(&NetworkAnalyser::SetPromiscuousMode),
              SetTimeout(&NetworkAnalyser::SetTimeout)
        {
        }
    };

protected:
    NetworkAnalyser *analyser;
    MockPacketQueue *queue;
    NetworkAnalyserPrivate *analyser_private;
    pcap_t *handle;

    std::optional<std::string> FindSuitableDevice();
};

/**
 * @brief Finds a suitable network device for testing.
 * @return An optional string representing the name of the selected network device, or std::nullopt if no suitable device is found.
 */
std::optional<std::string> NetworkAnalyserTests::FindSuitableDevice()
{
    char errbuf[PCAP_ERRBUF_SIZE];
    pcap_if_t *alldevs, *device;

    if (pcap_findalldevs(&alldevs, errbuf) == -1)
    {
        return std::nullopt;
    }

    for (device = alldevs; device != NULL; device = device->next)
    {
        if (device->flags & PCAP_IF_LOOPBACK)
        {
            continue;
        }

        std::string selected_device = device->name;
        pcap_freealldevs(alldevs);
        return selected_device;
    }

    pcap_freealldevs(alldevs);
    return std::nullopt;
}

/**
 * @brief Test case to verify that NetworkAnalyser constructor sets handle properly.
 */
TEST_F(NetworkAnalyserTests, ConstructorShouldSetHandle)
{
    ASSERT_NE(analyser->GetHandle(), nullptr);
}

/**
 * @brief Test case to verify that NetworkAnalyser constructor throws an exception on providing an invalid device.
 */
TEST_F(NetworkAnalyserTests, ConstructorShouldThrowOnInvalidDevice)
{
    const std::string device = "invalid_device";
    EXPECT_THROW({ new NetworkAnalyser(device, 1024, queue); }, NetworkAnalyserException);
}

/**
 * @brief Test case to verify that setting buffer size throws an exception when handle is activated.
 */
TEST_F(NetworkAnalyserTests, SetBufferSizeShouldThrowOnActivatedHandle)
{
    EXPECT_THROW((analyser->*analyser_private->SetBufferSize)(5 * 1024 * 1024), NetworkAnalyserException);
}

/**
 * @brief Test case to verify that setting snap length throws an exception when handle is activated.
 */
TEST_F(NetworkAnalyserTests, SetSnaplenShouldThrowOnActivatedHandle)
{
    EXPECT_THROW((analyser->*analyser_private->SetSnaplen)(), NetworkAnalyserException);
}

/**
 * @brief Test case to verify that setting promiscuous mode throws an exception when handle is activated.
 */
TEST_F(NetworkAnalyserTests, SetPromiscuousModeThrowOnActivatedHandle)
{
    EXPECT_THROW((analyser->*analyser_private->SetPromiscuousMode)(), NetworkAnalyserException);
}

/**
 * @brief Test case to verify that setting timeout throws an exception when handle is activated.
 */
TEST_F(NetworkAnalyserTests, SetTimeoutThrowOnActivatedHandle)
{
    EXPECT_THROW((analyser->*analyser_private->SetTimeout)(), NetworkAnalyserException);
}

/**
 * @brief Test case to verify that activating handle throws an exception when handle is already activated.
 */
TEST_F(NetworkAnalyserTests, ActivateHandleThrowOnActivatedHandle)
{
    EXPECT_THROW((analyser->*analyser_private->ActivateHandle)(), NetworkAnalyserException);
}

/**
 * @brief Test case to verify that network resources are released upon destruction of NetworkAnalyser object.
 */
TEST_F(NetworkAnalyserTests, HandleResourcesReleasedOnDestruction)
{
    std::optional<std::string> device_opt = FindSuitableDevice();
    if (!device_opt)
    {
        FAIL() << "No suitable network device found";
    }
    NetworkAnalyser *temp_analyser = new NetworkAnalyser(device_opt.value(), 1024 * 1024 * 5, queue);
    pcap_t *handle_before = temp_analyser->GetHandle();
    delete temp_analyser;
    EXPECT_EQ(pcap_fileno(handle_before), -1);
}

/**
 * @brief Test case to verify that StartCapture initiates packet capture.
 */
TEST_F(NetworkAnalyserTests, StartCaptureShouldInitiatePacketCapture)
{
    std::thread captureThread([&]()
                              { ASSERT_NO_THROW(analyser->StartCapture()); });

    std::this_thread::sleep_for(std::chrono::seconds(1));

    analyser->StopCapture();
    captureThread.join();
}

/**
 * @brief Test case to verify that StopCapture stops packet capture.
 */
TEST_F(NetworkAnalyserTests, StopCaptureShouldInitiatePacketCapture)
{
    std::thread captureThread([&]()
                              { analyser->StartCapture(); });

    std::this_thread::sleep_for(std::chrono::seconds(1));

    ASSERT_NO_THROW(analyser->StopCapture());
    captureThread.join();
}

/**
 * @brief Test case to verify that SetBpfFilter installs filter correctly.
 */
TEST_F(NetworkAnalyserTests, SetBpfFilterShouldInstallCorrectly)
{
    EXPECT_NO_THROW((analyser->*analyser_private->SetBpfFilter)());
}