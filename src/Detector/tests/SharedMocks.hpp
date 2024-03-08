/**
 * @file MockClasses.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Declaration of mock classes for unit testing with Google Mock.
 *
 * Provides mock implementations of interfaces such as IQueue, IDatabase, and IMessagePublisher,
 * facilitating isolated testing of system components that interact with these interfaces. By simulating
 * real-world interactions within a controlled test environment, these mocks enable precise verification
 * of component behavior under various scenarios.
 *
 * @version 1.0
 * @date 2024-03-04
 */

#pragma once

#include <gmock/gmock.h>
#include <gtest/gtest.h>
#include <unordered_set>
#include <string>
#include <map>

#include "DNSPacketInfo.hpp"
#include "IDatabase.hpp"
#include "IMessagePublisher.hpp"
#include "IQueue.hpp"
#include "DetectorPacket.hpp"
#include "ValidatedDomains.hpp"

/**
 * @class MockPacketQueue
 * @brief Mock class for IQueue interface specialized for Packet objects.
 *
 * Simulates a queue for Packet objects to test components that produce or consume
 * network packets without requiring a real queue implementation.
 */
class MockPacketQueue : public IQueue<DetectorPacket>
{
public:
    MOCK_METHOD(bool, try_pop, (DetectorPacket &), (override));
    MOCK_METHOD(void, emplace, (DetectorPacket &&), (override));
};

/**
 * @class MockDNSQueue
 * @brief Mock class for IQueue interface specialized for DNSPacketInfo objects.
 *
 * Provides a mock queue for DNSPacketInfo objects, allowing for the testing of DNS packet
 * processing logic in isolation from the actual queue mechanics and network operations.
 */
class MockDNSQueue : public IQueue<DNSPacketInfo>
{
public:
    MOCK_METHOD(bool, try_pop, (DNSPacketInfo &), (override));
    MOCK_METHOD(void, emplace, (DNSPacketInfo &&), (override));
};

/**
 * @class MockValidatedDomainsQueue
 * @brief Mock class for IQueue interface specialized for ValidatedDomains objects.
 *
 * Facilitates testing of systems that handle domain validation results by simulating
 * queue operations for ValidatedDomains objects, enabling interaction testing without
 * a real queue.
 */
class MockValidatedDomainsQueue : public IQueue<ValidatedDomains>
{
public:
    MOCK_METHOD(bool, try_pop, (ValidatedDomains &), (override));
    MOCK_METHOD(void, emplace, (ValidatedDomains &&), (override));
};

/**
 * @class MockDatabase
 * @brief Mock class for IDatabase interface.
 *
 * Simulates database operations, specifically for checking if domain names are present
 * in blacklists or whitelists. Enables testing of database interaction logic without
 * the need for a real database connection.
 */
class MockDatabase : public IDatabase
{
public:
    MOCK_METHOD((std::map<std::string, bool>), CheckInBlacklist, (const std::unordered_set<std::string> &), (override));
    MOCK_METHOD((std::map<std::string, bool>), CheckInWhitelist, (const std::unordered_set<std::string> &), (override));

private:
    MOCK_METHOD(void, HandleBlacklistHit, (const std::string &element), (override));
};

/**
 * @class MockMessagePublisher
 * @brief Mock class for IMessagePublisher interface.
 *
 * Allows for the testing of message publishing logic by simulating the behavior of a message
 * publisher, facilitating the examination of how messages are prepared and sent without
 * interacting with a real messaging service.
 */
class MockMessagePublisher : public IMessagePublisher
{
public:
    MOCK_METHOD(void, PublishMessage, (const std::string &), (const, override));
};
