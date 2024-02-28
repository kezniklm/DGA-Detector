/**
 * @file Publisher.cpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Implements the functionality for publishing validated domain information.
 *
 * The main functionalities of this file include:
 * - Processing validated domains by converting them to JSON and publishing messages.
 * - Converting a map of domain-return code pairs to JSON format.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 *
 */

#include "Publisher.hpp"

/**
 * @brief Processes validated domains by converting them to JSON and publishing messages.
 *
 * This function continuously processes validated domains by converting them to JSON format and publishing the messages.
 * It stops processing when the cancellation token is set.
 *
 * @returns None
 */
void Publisher::Process() const
{
    ValidatedDomains domains;
    while (!cancellation_token.load())
    {
        if (publisher_queue_->try_pop(domains))
        {
            nlohmann::json json_packet = ToJson(domains.domain_return_code_pairs_);

            std::string message_to_send = json_packet.dump(4);

            message_publisher_->PublishMessage(message_to_send);
        }
    }
}

/**
 * @brief Converts a map of domain-return code pairs to JSON format.
 *
 * This function converts a map of domain-return code pairs to JSON format.
 * Each domain and its return code are added as a new entry in the JSON object.
 *
 * @param domain_return_code_pairs The map containing domain-return code pairs.
 * @returns The JSON object representing the domain-return code pairs.
 */
nlohmann::json Publisher::ToJson(const std::unordered_map<std::string, int> &domain_return_code_pairs)
{
    nlohmann::json output_json;
    for (const auto &kPair : domain_return_code_pairs)
    {
        // Each domain and its return code is added as a new entry in the JSON object.
        output_json["domains"][kPair.first] = kPair.second;
    }
    return output_json;
}
