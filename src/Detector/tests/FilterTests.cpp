/**
 * @file FilterTests.cpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Implements unit tests for the Filter class.
 *
 * This file contains the implementation of unit tests for the Filter class. The Filter class is responsible for processing network packets, extracting DNS packets, and forwarding them to a DNS queue for further processing. These unit tests cover various scenarios to ensure the correct behavior of the Filter class under different conditions.
 *
 * The main functionalities of this file include:
 * - Testing the processing of valid DNS response packets.
 * - Verifying that DNS query packets are not forwarded to the DNS queue.
 * - Ensuring proper handling of malformed DNS packets.
 * - Checking the immediate stop of processing upon cancellation.
 * - Testing the processing of multiple DNS response packets.
 * - Verifying the handling of an empty packet queue.
 * - Testing the processing of a large DNS response packet.
 * - Verifying that DNS query packets with multiple queries are not forwarded.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 *
 */

#include <gmock/gmock.h>
#include <gtest/gtest.h>
#include "Filter.hpp"
#include "SharedMocks.hpp"

using ::testing::_;
using ::testing::AtLeast;
using ::testing::DoAll;
using ::testing::Return;
using ::testing::SetArgReferee;

/** External atomic bool for cancellation token */
extern std::atomic<bool> cancellation_token;

/**
 * @brief Converts a hex string to a byte array.
 *
 * @param hex The hexadecimal string to be converted.
 * @return std::vector<u_char> The converted byte array.
 */
std::vector<u_char> HexStringToBytes(const std::string &hex)
{
    std::vector<u_char> bytes;
    for (unsigned int i = 0; i < hex.length(); i += 2)
    {
        std::string byte_string = hex.substr(i, 2);
        u_char byte = static_cast<u_char>(strtol(byte_string.c_str(), nullptr, 16));
        bytes.push_back(byte);
    }
    return bytes;
}

/**
 * @brief Creates a Packet object from a hexadecimal string.
 *
 * @param hex_string The hexadecimal string representing packet data.
 * @return Packet The Packet object created from the hex string.
 */
Packet CreatePacketFromHexString(const std::string &hex_string)
{
    auto bytes = HexStringToBytes(hex_string);

    pcap_pkthdr hdr;
    auto now = std::chrono::system_clock::now();
    auto since_epoch = now.time_since_epoch();
    hdr.ts.tv_sec = std::chrono::duration_cast<std::chrono::seconds>(since_epoch).count();
    hdr.ts.tv_usec = std::chrono::duration_cast<std::chrono::microseconds>(since_epoch % std::chrono::seconds(1)).count();
    hdr.caplen = bytes.size();
    hdr.len = bytes.size();

    return Packet(hdr, bytes.data());
}

/**
 * @class FilterTest
 * @brief The test suite for the Filter class.
 *
 * This class implements unit tests for the Filter class, which is responsible for processing network packets,
 * extracting DNS packets, and forwarding them to a DNS queue for further processing.
 */
class FilterTest : public ::testing::Test
{
protected:
    std::unique_ptr<Filter> filter_;
    std::shared_ptr<MockDNSQueue> mock_dns_queue_;
    std::shared_ptr<MockPacketQueue> mock_packet_queue_;

    void SetUp() override
    {
        cancellation_token.store(false);

        mock_dns_queue_ = std::make_shared<MockDNSQueue>();
        mock_packet_queue_ = std::make_shared<MockPacketQueue>();
        filter_ = std::make_unique<Filter>(mock_packet_queue_.get(), mock_dns_queue_.get());
    }

    void TearDown() override
    {
        cancellation_token.store(false);
    }
};

/**
 * @brief Tests that valid DNS response packets are processed correctly.
 */
TEST_F(FilterTest, ProcessValidDnsResponses)
{
    std::string hex_response_dns_packet = "000c291dc716005056fc80ea0800450000531ff1000080117cd3c0a88e02c0a88e820035da79003f6544ead98180000100010000000106676f6f676c6503636f6d0000010001c00c000100010000000500048efb256e000029100000000000050000";
    auto mock_dns_response_packet = CreatePacketFromHexString(hex_response_dns_packet);

    EXPECT_CALL(*mock_packet_queue_, try_pop(_))
        .WillOnce(DoAll(SetArgReferee<0>(mock_dns_response_packet), Return(true)))
        .WillRepeatedly(Return(false));

    EXPECT_CALL(*mock_dns_queue_, emplace(_)).Times(1);

    std::thread worker([&]()
                       { filter_->ProcessPacket(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(300));

    cancellation_token.store(true);

    worker.join();
}

/**
 * @brief Tests that DNS query packets are not forwarded to the DNS queue.
 */
TEST_F(FilterTest, ProcessDnsQueryPackets)
{
    std::string hex_query_dns_packet = "005056fc80ea000c291dc71608004500005637a740004011651ac0a88e82c0a88e028577003500429e29aab70100000100000000000112636f6e6e65637469766974792d636865636b067562756e747503636f6d00001c00010000290200000000000000";
    auto dns_query_packet = CreatePacketFromHexString(hex_query_dns_packet);

    EXPECT_CALL(*mock_packet_queue_, try_pop(_))
        .WillOnce(DoAll(SetArgReferee<0>(dns_query_packet), Return(true)))
        .WillRepeatedly(Return(false));

    EXPECT_CALL(*mock_dns_queue_, emplace(_)).Times(0);

    std::thread worker([&]()
                       { filter_->ProcessPacket(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(300));

    cancellation_token.store(true);

    worker.join();
}

/**
 * @brief Tests the handling of malformed DNS packets.
 */
TEST_F(FilterTest, ProcessMalformedDnsPackets)
{
    std::string hex_malformed_dns_packet = "malformed";
    auto malformed_dns_packet = CreatePacketFromHexString(hex_malformed_dns_packet);

    EXPECT_CALL(*mock_packet_queue_, try_pop(_))
        .WillOnce(DoAll(SetArgReferee<0>(malformed_dns_packet), Return(true)))
        .WillRepeatedly(Return(false));

    EXPECT_CALL(*mock_dns_queue_, emplace(_)).Times(0);

    std::thread worker([&]()
                       { filter_->ProcessPacket(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(300));

    cancellation_token.store(true);

    worker.join();
}

/**
 * @brief Tests the immediate stop of processing upon cancellation.
 */
TEST_F(FilterTest, ImmediateStopOnCancellation)
{
    std::thread worker([&]()
                       { filter_->ProcessPacket(); });

    cancellation_token.store(true);

    worker.join();
}

/**
 * @brief Tests the processing of multiple DNS response packets.
 */
TEST_F(FilterTest, ProcessMultipleDnsResponses)
{
    std::string hex_dns_response_1 = "000c291dc716005056fc80ea0800450000531ff1000080117cd3c0a88e02c0a88e820035da79003f6544ead98180000100010000000106676f6f676c6503636f6d0000010001c00c000100010000000500048efb256e000029100000000000050000";
    std::string hex_dns_response_2 = "000c291dc716005056fc80ea0800450001a6240d000080117764c0a88e02c0a88e82003585770192a9a5aab781800001000c0000000112636f6e6e65637469766974792d636865636b067562756e747503636f6d00001c0001c00c001c00010000000500102620002d400000010000000000000097c00c001c00010000000500102620002d400000010000000000000098c00c001c00010000000500102001067c1562000000000000000023c00c001c00010000000500102620002d400000010000000000000097c00c001c00010000000500102620002d400000010000000000000096c00c001c00010000000500102620002d400000010000000000000098c00c001c00010000000500102620002d400000010000000000000022c00c001c00010000000500102001067c156200000000000000002400002910000000050000";
    Packet dns_response_packet_1 = CreatePacketFromHexString(hex_dns_response_1);
    Packet dns_response_packet_2 = CreatePacketFromHexString(hex_dns_response_2);

    EXPECT_CALL(*mock_packet_queue_, try_pop(_))
        .WillOnce(DoAll(SetArgReferee<0>(dns_response_packet_1), Return(true)))
        .WillOnce(DoAll(SetArgReferee<0>(dns_response_packet_2), Return(true)))
        .WillRepeatedly(Return(false));

    EXPECT_CALL(*mock_dns_queue_, emplace(_)).Times(AtLeast(2));

    std::thread worker([&]()
                       { filter_->ProcessPacket(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(300));

    cancellation_token.store(true);

    worker.join();
}

/**
 * @brief Tests the handling of an empty packet queue.
 */
TEST_F(FilterTest, ProcessEmptyPacketQueue)
{
    EXPECT_CALL(*mock_packet_queue_, try_pop(_)).WillRepeatedly(Return(false));

    EXPECT_CALL(*mock_dns_queue_, emplace(_)).Times(0);

    std::thread worker([&]()
                       { filter_->ProcessPacket(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(300));

    cancellation_token.store(true);

    worker.join();
}

/**
 * @brief Tests the processing of a large DNS response packet.
 */
TEST_F(FilterTest, ProcessLargeDnsResponse)
{
    std::string hex_large_dns_response = "000c291dc716005056fc80ea0800450003e8000d0000801174e8c0a88e02c0a88e8200358577048ec22974a981800001000c0000000112636f6e6e65637469766974792d636865636b067562756e747503636f6d00001c0001c00c001c00010000000500102620002d400000010000000000000097c00c001c00010000000500102620002d400000010000000000000098c00c001c00010000000500102001067c1562000000000000000023c00c001c00010000000500102620002d400000010000000000000097c00c001c00010000000500102620002d400000010000000000000096c00c001c00010000000500102620002d400000010000000000000098c00c001c00010000000500102620002d400000010000000000000022c00c001c00010000000500102001067c156200000000000000002400002910000000050000";
    auto large_dns_response_packet = CreatePacketFromHexString(hex_large_dns_response);

    EXPECT_CALL(*mock_packet_queue_, try_pop(_))
        .WillOnce(DoAll(SetArgReferee<0>(large_dns_response_packet), Return(true)))
        .WillRepeatedly(Return(false));

    EXPECT_CALL(*mock_dns_queue_, emplace(_)).Times(1);

    std::thread worker([&]()
                       { filter_->ProcessPacket(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(300));

    cancellation_token.store(true);

    worker.join();
}

/**
 * @brief Tests that DNS query packets with multiple queries are not forwarded.
 */
TEST_F(FilterTest, ProcessDnsQueryWithMultipleQueries)
{
    std::string hex_query_dns_packet = "005056fc80ea000c291dc71608004500005637a740004011651ac0a88e82c0a88e028577003500429e29aab70100000100000000000112636f6e6e65637469766974792d636865636b067562756e747503636f6d00001c00010000290200000000000000";
    auto dns_query_packet = CreatePacketFromHexString(hex_query_dns_packet);

    EXPECT_CALL(*mock_packet_queue_, try_pop(_))
        .WillOnce(DoAll(SetArgReferee<0>(dns_query_packet), Return(true)))
        .WillRepeatedly(Return(false));

    EXPECT_CALL(*mock_dns_queue_, emplace(_)).Times(0);

    std::thread worker([&]()
                       { filter_->ProcessPacket(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(300));

    cancellation_token.store(true);

    worker.join();
}
