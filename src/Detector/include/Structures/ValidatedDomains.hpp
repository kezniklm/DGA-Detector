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

/**
 * @struct ValidatedDomains
 * @brief Struct for storing validated domain-return code pairs.
 */
struct ValidatedDomains
{
    /** Map storing domain-return code pairs */
    std::unordered_map<std::string, int> domain_return_code_pairs_;

    /**
     * @brief Constructor with parameters.
     * @param domain_return_code_pairs The domain-return code pairs to initialize the struct.
     */
    ValidatedDomains(const std::unordered_map<std::string, int> domain_return_code_pairs) : domain_return_code_pairs_(domain_return_code_pairs) {}

    /**
     * @brief Default constructor.
     */
    ValidatedDomains() = default;
};