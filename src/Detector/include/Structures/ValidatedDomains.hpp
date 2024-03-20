/**
 * @file ValidatedDomains.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Declaration of the ValidatedDomains struct for storing validated domain-return code pairs.
 *
 * The main components of this file include:
 * - Declaration of the ValidatedDomains struct, responsible for storing validated domain-return code pairs.
 * - Declaration of member variables to store domain-return code pairs.
 * - Declaration of constructors for creating ValidatedDomains objects with provided domain-return code pairs.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 *
 */

#pragma once

#include <unordered_map>
#include <string>
#include <utility>

/**
 * @struct ValidatedDomains
 * @brief Manages domain-return code pairs.
 *
 * This struct maintains a collection of domain names and their associated return codes,
 * facilitating efficient access and management of this data.
 */
struct ValidatedDomains
{
    /**
     * @var domain_return_code_pairs_
     * @brief Stores domain and return code pairs.
     *
     * This unordered map stores domains as keys and their associated return codes as values.
     * It provides fast lookup of return codes based on domain names.
     */
    std::unordered_map<std::string, int> domain_return_code_pairs_;

    /**
     * @brief Constructs a ValidatedDomains object with given domain-return code pairs.
     * @param domain_return_code_pairs Map of domain-return code pairs for initialization.
     * @note This constructor is noexcept, ensuring it does not throw any exceptions.
     */
    ValidatedDomains(const std::unordered_map<std::string, int> &domain_return_code_pairs) noexcept
        : domain_return_code_pairs_(domain_return_code_pairs) {}

    /**
     * @brief Default move constructor.
     * @note Utilizes default noexcept behavior for efficient container operations.
     */
    ValidatedDomains(ValidatedDomains &&other) noexcept = default;

    /**
     * @brief Default copy constructor.
     * @note Utilizes default noexcept behavior for safe exception handling.
     */
    ValidatedDomains(const ValidatedDomains &other) noexcept = default;

    /**
     * @brief Default constructor.
     * Constructs an empty ValidatedDomains object.
     */
    ValidatedDomains() = default;

    /**
     * @brief Default move assignment operator.
     * @return Reference to the modified ValidatedDomains object.
     * @note Utilizes default noexcept behavior for efficient container operations.
     */
    ValidatedDomains &operator=(ValidatedDomains &&other) noexcept = default;

    /**
     * @brief Default copy assignment operator.
     * @return Reference to the modified ValidatedDomains object.
     * @note Utilizes default noexcept behavior for safe exception handling.
     */
    ValidatedDomains &operator=(const ValidatedDomains &other) noexcept = default;
};