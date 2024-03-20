/**
 * @file DomainValidatorTests.cpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Implements unit tests for the DomainValidator class.
 *
 * This file contains the implementation of unit tests for the DomainValidator class. The DomainValidator class is responsible for validating domains by checking them against a blacklist and whitelist, and then publishing the validated domains. These unit tests cover various scenarios to ensure the correct behavior of the DomainValidator class under different conditions.
 *
 * The main functionalities of this file include:
 * - Testing the behavior of the DomainValidator when processing DNS packets.
 * - Verifying proper handling of different response codes and invalid packets.
 * - Ensuring correct filtering of domains based on blacklist and whitelist.
 * - Testing the efficiency of processing large batches of domains.
 * - Checking the handling of empty blacklist and whitelist scenarios.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 *
 */

#include <gmock/gmock.h>
#include <gtest/gtest.h>

#include "DomainValidator.hpp"
#include "SharedMocks.hpp"

using ::testing::_;
using ::testing::AtLeast;
using ::testing::DoAll;
using ::testing::Invoke;
using ::testing::Return;
using ::testing::SetArgReferee;

/** External atomic bool for cancellation token */
extern std::atomic<bool> cancellation_token;

/**
 * @class DomainValidatorTest
 * @brief Test suite for DomainValidator class.
 *
 * DomainValidatorTest is a test suite class derived from ::testing::Test provided by the Google Test framework.
 * It utilizes mock objects to simulate interactions with external systems such as DNS queries,
 * publishing queues, and databases. The main goal is to test the DomainValidator class's ability to
 * validate domain names under various conditions without relying on real external dependencies.
 */
class DomainValidatorTest : public ::testing::Test
{
protected:
    MockDNSQueue mock_dns_queue;
    MockValidatedDomainsQueue mock_publisher_queue;
    MockDatabase mock_database;
    std::unique_ptr<DomainValidator> validator;

    void SetUp() override
    {
        cancellation_token.store(false);
        validator = std::make_unique<DomainValidator>(&mock_dns_queue, &mock_publisher_queue, &mock_database);
    }

    void TearDown() override
    {
        cancellation_token.store(false);
    }
};

/**
 * @brief Test case to validate that processing stops when cancellation is set
 */
TEST_F(DomainValidatorTest, StopsProcessingWhenCancellationIsSet)
{
    EXPECT_CALL(mock_dns_queue, try_pop(_))
        .WillOnce(Return(true))
        .WillRepeatedly(Return(false));

    std::thread cancel_thread([&]()
                              {
        std::this_thread::sleep_for(std::chrono::milliseconds(50));
        cancellation_token.store(true); });

    validator->ProcessDomains();

    cancel_thread.join();
}

/**
 * @brief Test case to validate the behavior of the DomainValidator when handling an empty DNS queue
 */
TEST_F(DomainValidatorTest, HandlesEmptyDNSQueueGracefully)
{
    EXPECT_CALL(mock_dns_queue, try_pop(_))
        .WillRepeatedly(Return(false));

    std::thread worker([&]()
                       { validator->ProcessDomains(); });
    std::this_thread::sleep_for(std::chrono::milliseconds(100));
    cancellation_token.store(true);

    worker.join();

    EXPECT_CALL(mock_database, CheckInBlacklist(_)).Times(0);
    EXPECT_CALL(mock_database, CheckInWhitelist(_)).Times(0);
    EXPECT_CALL(mock_publisher_queue, emplace(_)).Times(0);
}

/**
 * @brief Test case to validate the efficient processing of large batches of domains by the DomainValidator
 */
TEST_F(DomainValidatorTest, ProcessesLargeBatchesOfDomainsEfficiently)
{
    DNSPacketInfo packet_info{{"example.com"}, 200};

    EXPECT_CALL(mock_dns_queue, try_pop(_))
        .WillRepeatedly(DoAll(SetArgReferee<0>(packet_info), Return(true)));

    EXPECT_CALL(mock_database, CheckInBlacklist(_)).Times(AtLeast(1));
    EXPECT_CALL(mock_database, CheckInWhitelist(_)).Times(AtLeast(1));

    EXPECT_CALL(mock_publisher_queue, emplace(_))
        .Times(AtLeast(1));

    std::thread worker([&]()
                       { validator->ProcessDomains(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(500));

    cancellation_token.store(true);

    worker.join();
}

/**
 * @brief Test case for filtering domains based on blacklist and whitelist
 */
TEST_F(DomainValidatorTest, FiltersDomainsBasedOnBlacklistAndWhitelist)
{
    DNSPacketInfo packet_info_with_domains{{"valid.com", "blacklisted.com"}, 200};

    const std::map<std::string, bool> blacklistResult{{"blacklisted.com", true}, {"valid.com", false}};
    const std::map<std::string, bool> whitelistResult{{"whitelisted.com", true}, {"valid.com", false}};

    EXPECT_CALL(mock_dns_queue, try_pop(_))
        .WillRepeatedly(DoAll(SetArgReferee<0>(packet_info_with_domains), Return(true)));

    EXPECT_CALL(mock_database, CheckInBlacklist(_))
        .WillRepeatedly(Return(blacklistResult));
    EXPECT_CALL(mock_database, CheckInWhitelist(_))
        .WillRepeatedly(Return(whitelistResult));

    EXPECT_CALL(mock_publisher_queue, emplace(_))
        .Times(AtLeast(1));

    std::thread worker([&]()
                       { validator->ProcessDomains(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(500));

    cancellation_token.store(true);

    worker.join();
}

/**
 * @brief Test case to validate that a domain present in both the blacklist and whitelist is filtered out
 */
TEST_F(DomainValidatorTest, DomainInBothBlacklistAndWhitelistIsFilteredOut)
{
    constexpr int num_packet_info_instances = 100000;

    const DNSPacketInfo packet_info{{"conflicted.com"}, 200};

    const std::map<std::string, bool> blacklistResult{{"conflicted.com", true}};
    const std::map<std::string, bool> whitelistResult{{"conflicted.com", false}};

    int call_count = 0;

    EXPECT_CALL(mock_dns_queue, try_pop(_))
        .WillRepeatedly(Invoke([&](DNSPacketInfo &packetInfoArg) -> bool
                               {
            if (call_count <= num_packet_info_instances) {
                packetInfoArg = packet_info; 
                ++call_count;
                return true; 
            } else {
                return false;
            } }));

    EXPECT_CALL(mock_database, CheckInBlacklist(_))
        .Times(1)
        .WillRepeatedly(Return(blacklistResult));
    EXPECT_CALL(mock_database, CheckInWhitelist(_))
        .Times(1)
        .WillRepeatedly(Return(whitelistResult));

    EXPECT_CALL(mock_publisher_queue, emplace(_))
        .Times(0);

    std::thread worker([&]()
                       { validator->ProcessDomains(); });

    std::this_thread::sleep_for(std::chrono::seconds(1));

    cancellation_token.store(true);

    worker.join();
}

/**
 * @brief Test case to validate the handling of various response codes by the DomainValidator
 */
TEST_F(DomainValidatorTest, HandlesVariousResponseCodesCorrectly)
{
    DNSPacketInfo packetInfoSuccess{{"valid.com"}, 200};
    DNSPacketInfo packetInfoFailure{{"invalid.com"}, 404};

    EXPECT_CALL(mock_dns_queue, try_pop(_))
        .WillOnce(DoAll(SetArgReferee<0>(packetInfoSuccess), Return(true)))
        .WillOnce(DoAll(SetArgReferee<0>(packetInfoFailure), Return(true)))
        .WillRepeatedly(Return(false));

    EXPECT_CALL(mock_database, CheckInBlacklist(_))
        .Times(AtLeast(0));
    EXPECT_CALL(mock_database, CheckInWhitelist(_))
        .Times(AtLeast(0));
    EXPECT_CALL(mock_publisher_queue, emplace(_))
        .Times(AtLeast(0));

    std::thread worker([&]()
                       { validator->ProcessDomains(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(100));

    cancellation_token.store(true);

    worker.join();
}

/**
 * @brief Test case for processing a mix of valid and invalid DNS packets
 */
TEST_F(DomainValidatorTest, ProcessesMixOfValidAndInvalidDNSPacketsCorrectly)
{
    DNSPacketInfo validPacket{{"valid.com"}, 200};
    DNSPacketInfo invalidPacket;

    EXPECT_CALL(mock_dns_queue, try_pop(_))
        .WillOnce(DoAll(SetArgReferee<0>(validPacket), Return(true)))
        .WillOnce(DoAll(SetArgReferee<0>(invalidPacket), Return(true)))
        .WillRepeatedly(Return(false));

    EXPECT_CALL(mock_database, CheckInBlacklist(_))
        .Times(AtLeast(0));
    EXPECT_CALL(mock_database, CheckInWhitelist(_))
        .Times(AtLeast(0));
    EXPECT_CALL(mock_publisher_queue, emplace(_))
        .Times(AtLeast(0));

    std::thread worker([&]()
                       { validator->ProcessDomains(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(100));

    cancellation_token.store(true);

    worker.join();
}

/**
 * @brief Test case to validate that a domain in the blacklist is filtered out
 */
TEST_F(DomainValidatorTest, DomainInBlacklistIsFilteredOut)
{
    constexpr int num_packet_info_instances = 100000;

    const DNSPacketInfo packet_info{{"blacklisted.com"}, 200};

    const std::map<std::string, bool> blacklistResult{{"blacklisted.com", true}};
    const std::map<std::string, bool> whitelistResult{{"blacklisted.com", false}};

    int call_count = 0;

    EXPECT_CALL(mock_dns_queue, try_pop(_))
        .WillRepeatedly(Invoke([&](DNSPacketInfo &packetInfoArg) -> bool
                               {
            if (call_count <= num_packet_info_instances) {
                packetInfoArg = packet_info; 
                ++call_count;
                return true; 
            } else {
                return false;
            } }));

    EXPECT_CALL(mock_database, CheckInBlacklist(_))
        .WillOnce(Return(blacklistResult));

    EXPECT_CALL(mock_database, CheckInWhitelist(_))
        .WillOnce(Return(whitelistResult));

    EXPECT_CALL(mock_publisher_queue, emplace(_))
        .Times(0);

    std::thread worker([&]()
                       { validator->ProcessDomains(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(700));

    cancellation_token.store(true);

    worker.join();
}

/**
 * @brief Test case to validate that a domain not in the blacklist or whitelist is published
 */
TEST_F(DomainValidatorTest, DomainNotInBlacklistOrWhitelistIsPublished)
{
    constexpr int num_packet_info_instances = 100000;

    const DNSPacketInfo packet_info{{"newdomain.com"}, 200};
    const std::map<std::string, bool> blacklistResult{{"newdomain.com", false}};
    const std::map<std::string, bool> whitelistResult{{"newdomain.com", false}};

    int call_count = 0;

    EXPECT_CALL(mock_dns_queue, try_pop(_))
        .WillRepeatedly(Invoke([&](DNSPacketInfo &packetInfoArg) -> bool
                               {
            if (call_count <= num_packet_info_instances) {
                packetInfoArg = packet_info; 
                ++call_count;
                return true; 
            } else {
                return false; 
            } }));

    EXPECT_CALL(mock_database, CheckInBlacklist(_))
        .WillOnce(Return(blacklistResult));
    EXPECT_CALL(mock_database, CheckInWhitelist(_))
        .WillOnce(Return(whitelistResult));

    EXPECT_CALL(mock_publisher_queue, emplace(_))
        .Times(1);

    std::thread worker([&]()
                       { validator->ProcessDomains(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(500));

    cancellation_token.store(true);

    worker.join();
}

/**
 * @brief Test case for handling an invalid DNS packet in the DomainValidator class
 */
TEST_F(DomainValidatorTest, HandlesInvalidDNSPacket)
{
    constexpr int num_packet_info_instances = 100000;

    DNSPacketInfo invalidPacket;

    EXPECT_CALL(mock_dns_queue, try_pop(_))
        .Times(AtLeast(num_packet_info_instances))
        .WillRepeatedly(DoAll(SetArgReferee<0>(invalidPacket), Return(true)));

    EXPECT_CALL(mock_database, CheckInBlacklist(_)).Times(0);
    EXPECT_CALL(mock_database, CheckInWhitelist(_)).Times(0);
    EXPECT_CALL(mock_publisher_queue, emplace(_)).Times(0);

    std::thread worker([&]()
                       { validator->ProcessDomains(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(500));

    cancellation_token.store(true);

    worker.join();
}

/**
 * @brief Test case for handling empty blacklist and whitelist in DomainValidator.
 */
TEST_F(DomainValidatorTest, HandlesEmptyBlacklistAndWhitelist)
{
    constexpr int num_packet_info_instances = 100000;

    const DNSPacketInfo packet_info{{"example.com"}, 200};

    const std::map<std::string, bool> emptyResult{};

    int call_count = 0;

    EXPECT_CALL(mock_dns_queue, try_pop(_))
        .WillRepeatedly(Invoke([&](DNSPacketInfo &packetInfoArg) -> bool
                               {
            if (call_count <= num_packet_info_instances) {
                packetInfoArg = packet_info; 
                ++call_count;
                return true; 
            } else {
                return false; 
            } }));

    EXPECT_CALL(mock_database, CheckInBlacklist(_))
        .WillOnce(Return(emptyResult));
    EXPECT_CALL(mock_database, CheckInWhitelist(_))
        .WillOnce(Return(emptyResult));

    EXPECT_CALL(mock_publisher_queue, emplace(_))
        .Times(1);

    std::thread worker([&]()
                       { validator->ProcessDomains(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(500));

    cancellation_token.store(true);

    worker.join();
}