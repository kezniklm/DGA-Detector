/**
 * @file DomainValidator.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Declares the DomainValidator class for processing DNS packet information.
 *
 * The main functionalities of this class include:
 * - Processing domains by continuously processing DNS packet information until a cancellation token is set.
 * - Processing packet information by updating domain-return code pairs.
 * - Determining whether to process a batch of domains based on the current batch size and cycle count.
 * - Processing a batch of domains by performing blacklist and whitelist checks and publishing validated domains.
 * - Removing listed domains from the domain-return code pairs based on blacklist and whitelist checks.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 *
 */

#pragma once

#include <atomic>
#include <chrono>
#include <map>
#include <thread>
#include <unordered_map>
#include <unordered_set>

#include "DNSPacketInfo.hpp"
#include "IDatabase.hpp"
#include "IQueue.hpp"
#include "MessagePublisher.hpp"
#include "ValidatedDomains.hpp"

/** External atomic bool for cancellation token */
extern std::atomic<bool> cancellation_token;

/**
 * @class DomainValidator
 * @brief Processes DNS packet information and validates domains with the use of Blacklist and Whitelist
 */
class DomainValidator
{
public:
    /**
     * @brief Constructs a DomainValidator object.
     *
     * @param dns_info_queue Pointer to the MPMCQueue containing DNS packet information.
     * @param publisher_queue Pointer to the MPMCQueue for publishing validated domains.
     * @param db Pointer to the Database instance for blacklist and whitelist checks.
     */
    explicit DomainValidator(IQueue<DNSPacketInfo> *dns_info_queue,
                             IQueue<ValidatedDomains> *publisher_queue,
                             IDatabase *database)
        : dns_info_queue_(dns_info_queue), publisher_queue_(publisher_queue), database_(database) {}

    /**
     * @brief Processes domains by continuously processing DNS packet information until a cancellation token is set.
     *
     * This function continuously processes DNS packet information from the DNS information queue until a cancellation token is set. It processes packet information, updates domain-return code pairs, and processes batches of domains.
     */
    void ProcessDomains();

private:
    /**
     * @brief Processes packet information by updating domain-return code pairs.
     *
     * This function processes packet information by extracting domain names and their corresponding response codes from the provided DNS packet information. It updates the domain-return code pairs and increments the cycle count.
     *
     * @param packet_info The DNS packet information containing domain names and response codes.
     * @param domain_return_code_pairs The map to store domain-return code pairs.
     * @param cycle_count The count of cycles processed.
     */
    void ProcessPacketInfo(const DNSPacketInfo &packet_info,
                           std::unordered_map<std::string, int> &domain_return_code_pairs,
                           int &cycle_count);

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
    bool ShouldProcessBatch(const size_t current_batch_size, const unsigned cycle_count, const unsigned max_batch_size, const unsigned max_cycle_count) const;

    /**
     * @brief Processes a batch of domains by performing blacklist and whitelist checks and publishing validated domains.
     *
     * This function processes a batch of domains by performing blacklist and whitelist checks using the database. It removes listed domains from the domain-return code pairs and publishes the validated domains to the publisher queue.
     *
     * @param domain_return_code_pairs The map containing domain-return code pairs to process.
     */
    void ProcessBatch(std::unordered_map<std::string, int> &domain_return_code_pairs);

    /**
     * @brief Removes listed domains from the domain-return code pairs based on blacklist and whitelist checks.
     *
     * This function removes listed domains from the domain-return code pairs based on the blacklist and whitelist check results obtained from the database.
     *
     * @param domain_return_code_pairs The map containing domain-return code pairs.
     * @param result_list The map containing the results of the blacklist or whitelist check.
     */
    void RemoveListedDomains(std::unordered_map<std::string, int> &domain_return_code_pairs,
                             const std::map<std::string, bool> &result_list);

    /** Pointer to the DNS packet information queue */
    IQueue<DNSPacketInfo> *dns_info_queue_;

    /** Pointer to the validated domains queue */
    IQueue<ValidatedDomains> *publisher_queue_;

    /** Pointer to the database */
    IDatabase *database_;

    static constexpr int DEFAULT_SIZE = 100000;

    static constexpr int MAX_BATCH_SIZE = DEFAULT_SIZE;

    static constexpr int MAX_CYCLE_COUNT = DEFAULT_SIZE;
};
