/**
 * @file PublisherTests.cpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Implements unit tests for the Publisher class.
 *
 * This file contains the implementation of unit tests for the Publisher class. The Publisher class is responsible for processing validated domain data and publishing it as JSON messages. These unit tests cover various scenarios to ensure the correct behavior of the Publisher class under different conditions.
 *
 * The main functionalities of this file include:
 * - Testing the handling of an empty queue.
 * - Verifying the processing of a non-empty queue and the publishing of messages.
 * - Ensuring the Publisher stops processing when cancellation is requested.
 * - Testing the processing of multiple domains and publishing multiple messages.
 * - Verifying the integrity of JSON format for special characters in domain names.
 * - Ensuring the graceful execution of shutdown procedures.
 * - Verifying the handling of empty messages.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 *
 */

#include <gtest/gtest.h>
#include <gmock/gmock.h>

#include "nlohmann/json.hpp"

#include "Publisher.hpp"
#include "SharedMocks.hpp"

using ::testing::_;
using ::testing::AtLeast;
using ::testing::DoAll;
using ::testing::Invoke;
using ::testing::Return;
using ::testing::SetArgReferee;

/**
 * @brief Compares two JSON strings for equality.
 * @param actualJsonStr The actual JSON string.
 * @param expectedJsonStr The expected JSON string.
 * @return True if the JSON strings represent the same document; otherwise, false.
 */
bool JsonEqual(const std::string &actualJsonStr, const std::string &expectedJsonStr)
{
    try
    {
	    const auto actualJson = nlohmann::json::parse(actualJsonStr);
	    const auto expectedJson = nlohmann::json::parse(expectedJsonStr);
        return actualJson == expectedJson;
    }
    catch (const std::exception &e)
    {
        std::cerr << "JSON comparison failed with error: " << e.what() << std::endl;
        return false;
    }
}

/** External atomic bool for cancellation token */
extern std::atomic<bool> cancellation_token;

/**
 * @brief Test fixture for Publisher tests.
 */
class PublisherTest : public ::testing::Test
{
protected:
    MockValidatedDomainsQueue mock_validated_domains_queue;
    MockMessagePublisher mock_message_publisher;
    std::unique_ptr<Publisher> publisher;

    void SetUp() override
    {
        cancellation_token.store(false);
        publisher = std::make_unique<Publisher>(&mock_validated_domains_queue, &mock_message_publisher);
    }

    void TearDown() override
    {
        cancellation_token.store(false);
    }
};

/**
 * @brief Tests handling of an empty queue by the Publisher.
 */
TEST_F(PublisherTest, HandlesEmptyQueueGracefully)
{
    EXPECT_CALL(mock_validated_domains_queue, try_pop(_)).WillRepeatedly(Return(false));

    std::thread testThread([this]()
                           { publisher->Process(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(300));
    cancellation_token.store(true);
    testThread.join();
}

/**
 * @brief Tests processing of a non-empty queue and publishing of messages by the Publisher.
 */
TEST_F(PublisherTest, ProcessesNonEmptyQueueAndPublishesMessages)
{
    ValidatedDomains validatedDomains{{{"example.com", 200}, {"test.com", 404}}};
    std::string expectedJson = R"({"domains":{"example.com":200,"test.com":404}})";

    EXPECT_CALL(mock_validated_domains_queue, try_pop(_))
        .WillOnce(DoAll(SetArgReferee<0>(validatedDomains), Return(true)))
        .WillRepeatedly(Return(false));

    EXPECT_CALL(mock_message_publisher, PublishMessage(::testing::Truly([expectedJson](const std::string &actualJsonStr)
                                                                        { return JsonEqual(actualJsonStr, expectedJson); })))
        .Times(1);

    std::thread testThread([this]()
                           { publisher->Process(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(100));
    cancellation_token.store(true);
    testThread.join();
}

/**
 * @brief Tests that the Publisher stops processing when cancellation is requested.
 */
TEST_F(PublisherTest, StopsProcessingWhenCancellationIsRequested)
{
    EXPECT_CALL(mock_validated_domains_queue, try_pop(_)).WillRepeatedly(Return(true));
    EXPECT_CALL(mock_message_publisher, PublishMessage(_)).Times(0);

    std::thread testThread([this]()
                           { publisher->Process(); });
    cancellation_token.store(true);
    testThread.join();
}

/**
 * @brief Tests processing of multiple domains and publishing multiple messages by the Publisher.
 */
TEST_F(PublisherTest, ProcessesMultipleDomainsAndPublishesMultipleMessages)
{
    ValidatedDomains firstBatch{{{"first.com", 200}}};
    ValidatedDomains secondBatch{{{"second.com", 404}}};

    EXPECT_CALL(mock_validated_domains_queue, try_pop(_))
        .WillOnce(DoAll(SetArgReferee<0>(firstBatch), Return(true)))
        .WillOnce(DoAll(SetArgReferee<0>(secondBatch), Return(true)))
        .WillRepeatedly(Return(false));

    EXPECT_CALL(mock_message_publisher, PublishMessage(_)).Times(2);

    std::thread testThread([this]()
                           { publisher->Process(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(200));
    cancellation_token.store(true);
    testThread.join();
}

/**
 * @brief Tests JSON format integrity for special characters in domain names.
 */
TEST_F(PublisherTest, EnsuresJSONFormatIntegrity)
{
    ValidatedDomains domainsWithSpecialCharacters{{{"example.com/somepath", 200}, {"test.com?query=1", 404}}};
    std::string expectedJson = R"({"domains":{"example.com/somepath":200,"test.com?query=1":404}})";

    EXPECT_CALL(mock_validated_domains_queue, try_pop(_))
        .WillOnce(DoAll(SetArgReferee<0>(domainsWithSpecialCharacters), Return(true)))
        .WillRepeatedly(Return(false));

    EXPECT_CALL(mock_message_publisher, PublishMessage(::testing::Truly([expectedJson](const std::string &actualJson)
                                                                        {
                                                                            const auto expected = nlohmann::json::parse(expectedJson);
                                                                            const auto actual = nlohmann::json::parse(actualJson);
                                                                            EXPECT_EQ(expected, actual);
                                                                            return true; })))
        .Times(1);

    std::thread testThread([this]()
                           { publisher->Process(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(100));
    cancellation_token.store(true);
    testThread.join();
}

/**
 * @brief Tests the graceful execution of shutdown procedures by the Publisher.
 */
TEST_F(PublisherTest, ExecutesShutdownProcedureGracefully)
{
    EXPECT_CALL(mock_validated_domains_queue, try_pop(_)).WillRepeatedly(Return(false));

    std::thread testThread([this]()
                           { publisher->Process(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(50));
    cancellation_token.store(true);
    testThread.join();
}

/**
 * @brief Tests handling of empty messages by the Publisher.
 */
TEST_F(PublisherTest, HandlesEmptyMessages)
{
    ValidatedDomains emptyDomains;

    EXPECT_CALL(mock_validated_domains_queue, try_pop(_))
        .WillOnce(DoAll(SetArgReferee<0>(emptyDomains), Return(true)))
        .WillRepeatedly(Return(false));

    EXPECT_CALL(mock_message_publisher, PublishMessage(_)).Times(0);

    std::thread testThread([this]()
                           { publisher->Process(); });

    std::this_thread::sleep_for(std::chrono::milliseconds(500));
    cancellation_token.store(true);
    testThread.join();
}
