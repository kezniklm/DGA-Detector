/**
 * @file DomainValidator.cpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Implementation of the DomainValidator class for processing DNS packet information and validating domain names.
 *
 * This file defines the behavior of the DomainValidator class, which is responsible for processing DNS packet information and validating domain names. It contains member functions to process domains, process packet information, determine whether to process a batch of domains, process a batch of domains, and remove listed domains based on blacklist and whitelist checks.
 *
 * The main functionalities of this file include:
 * - Processing domains by continuously processing DNS packet information until a cancellation token is set.
 * - Processing packet information by extracting domain names and their corresponding response codes.
 * - Determining whether to process a batch of domains based on the current batch size and cycle count.
 * - Processing a batch of domains by performing blacklist and whitelist checks and removing listed domains.
 * - Removing listed domains from the domain-return code pairs based on blacklist and whitelist checks.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 * 
 */

#include "DomainValidator.hpp"

#include "NetworkAnalyser.hpp"

using namespace std;

/**
 * @brief Processes domains by continuously processing DNS packet information until a cancellation token is set.
 *
 * This function continuously processes DNS packet information from the DNS information queue until a cancellation token is set. It processes packet information, updates domain-return code pairs, and processes batches of domains.
 */
void DomainValidator::ProcessDomains()
{
    unordered_map<string, int> domain_return_code_pairs;

    domain_return_code_pairs.reserve(MAX_BATCH_SIZE);

    int cycle_count = 0;

    while (!cancellation_token.load())
    {
        DNSPacketInfo packet_info;
        if (dns_info_queue_->try_pop(packet_info))
        {
            ProcessPacketInfo(packet_info, domain_return_code_pairs, cycle_count);

            if (ShouldProcessBatch(domain_return_code_pairs.size(), cycle_count, MAX_BATCH_SIZE, MAX_CYCLE_COUNT))
            {
                ProcessBatch(domain_return_code_pairs);
                cycle_count = 0; // Reset cycle count for the next batch
            }
        }
        else
        {
            this_thread::sleep_for(chrono::milliseconds(100)); // Add sleep to reduce CPU usage
        }
    }
}

/**
 * @brief Processes packet information by updating domain-return code pairs.
 *
 * This function processes packet information by extracting domain names and their corresponding response codes from the provided DNS packet information. It updates the domain-return code pairs and increments the cycle count.
 *
 * @param packet_info The DNS packet information containing domain names and response codes.
 * @param domain_return_code_pairs The map to store domain-return code pairs.
 * @param cycle_count The count of cycles processed.
 */
void DomainValidator::ProcessPacketInfo(const DNSPacketInfo &packet_info,
                                        unordered_map<string, int> &domain_return_code_pairs,
                                        int &cycle_count)
{
    for (const auto &kDomainName : packet_info.domain_names)
    {
        domain_return_code_pairs[kDomainName] = packet_info.response_code;
        cycle_count++;
    }
}

/**
 * @brief Determines whether to process a batch of domains based on the current batch size and cycle count.
 *
 * This function determines whether to process a batch of domains based on the current batch size and cycle count compared to the maximum batch size and maximum cycle count. If the current batch size exceeds the maximum batch size or the cycle count exceeds the maximum cycle count, it returns true indicating that a batch should be processed.
 *
 * @param current_batch_size The current size of the domain-return code pairs.
 * @param cycle_count The count of cycles processed.
 * @param max_batch_size The maximum allowed batch size.
 * @param max_cycle_count The maximum allowed cycle count.
 * @return True if a batch should be processed, false otherwise.
 */
bool DomainValidator::ShouldProcessBatch(const size_t current_batch_size,
                                         const unsigned cycle_count,
                                         const unsigned max_batch_size,
                                         const unsigned max_cycle_count) const
{
    return current_batch_size >= max_batch_size || cycle_count > max_cycle_count;
}

/**
 * @brief Processes a batch of domains by performing blacklist and whitelist checks and publishing validated domains.
 *
 * This function processes a batch of domains by performing blacklist and whitelist checks using the database. It removes listed domains from the domain-return code pairs and publishes the validated domains to the publisher queue.
 *
 * @param domain_return_code_pairs The map containing domain-return code pairs to process.
 */
void DomainValidator::ProcessBatch(std::unordered_map<std::string, int> &domain_return_code_pairs)
{
    std::unordered_set<std::string> domain_names_to_query;
    for (const auto &kPair : domain_return_code_pairs)
    {
        domain_names_to_query.insert(kPair.first);
    }

    const auto result_blacklist_check = database_->CheckInBlacklist(domain_names_to_query);
    const auto result_whitelist_check = database_->CheckInWhitelist(domain_names_to_query);

    RemoveListedDomains(domain_return_code_pairs, result_blacklist_check);
    RemoveListedDomains(domain_return_code_pairs, result_whitelist_check);

    publisher_queue_->emplace(ValidatedDomains{
        domain_return_code_pairs}); // Assuming ValidatedDomains can be constructed this way

    domain_return_code_pairs.clear();
}

/**
 * @brief Removes listed domains from the domain-return code pairs based on blacklist and whitelist checks.
 *
 * This function removes listed domains from the domain-return code pairs based on the blacklist and whitelist check results obtained from the database.
 *
 * @param domain_return_code_pairs The map containing domain-return code pairs.
 * @param result_list The map containing the results of the blacklist or whitelist check.
 */
void DomainValidator::RemoveListedDomains(std::unordered_map<std::string, int> &domain_return_code_pairs,
                                          const std::map<std::string, bool> &result_list)
{
    for (const auto &kResult : result_list)
    {
        if (kResult.second)
        {
            domain_return_code_pairs.erase(kResult.first);
        }
    }
}