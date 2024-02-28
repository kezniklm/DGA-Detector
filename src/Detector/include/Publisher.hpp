/**
 * @file Publisher.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Declares the Publisher class for publishing validated domain information.
 *
 * The main functionalities of this class include:
 * - Processing validated domains by converting them to JSON and publishing messages.
 * - Converting a map of domain-return code pairs to JSON format.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 *
 */

#pragma once

#include <string>
#include <unordered_map>

#include "nlohmann/json.hpp"
#include "rigtorp/MPMCQueue.h"

#include "DNSPacketInfo.hpp"
#include "MessagePublisher.hpp"
#include "ValidatedDomains.hpp"

/** External atomic bool for cancellation token */
extern std::atomic<bool> cancellation_token;

/**
 * @brief Publisher class for publishing validated domain information into RabbitMQ
 */
class Publisher
{
public:
    /**
     * @brief Constructs a Publisher object with a publisher queue and a message publisher.
     *
     * @param publisher_queue The MPMCQueue for publishing validated domains.
     * @param message_publisher The message publisher for publishing messages.
     */
    explicit Publisher(rigtorp::MPMCQueue<ValidatedDomains> *publisher_queue, MessagePublisher *message_publisher)
        : publisher_queue_(publisher_queue), message_publisher_(message_publisher) {}

    /**
     * @brief Processes validated domains by converting them to JSON and publishing messages.
     *
     * This function continuously processes validated domains by converting them to JSON format and publishing the messages.
     * It stops processing when the cancellation token is set.
     *
     * @returns None
     */
    void Process() const;

private:
    rigtorp::MPMCQueue<ValidatedDomains> *publisher_queue_;
    MessagePublisher *message_publisher_;

    /**
     * @brief Converts a map of domain-return code pairs to JSON format.
     *
     * This function converts a map of domain-return code pairs to JSON format.
     * Each domain and its return code are added as a new entry in the JSON object.
     *
     * @param domain_return_code_pairs The map containing domain-return code pairs.
     * @returns The JSON object representing the domain-return code pairs.
     */
    static nlohmann::json ToJson(const std::unordered_map<std::string, int> &domain_return_code_pairs);
};