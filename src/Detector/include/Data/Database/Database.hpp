/**
 * @file Database.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Declaration of the Database class for handling blacklist and whitelist checks.
 *
 * This file declares the Database class, which provides interfaces for handling blacklist hits and performing blacklist and whitelist checks.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 *
 */

#pragma once

#include <string>

/**
 * @class Database
 * @brief Interface for handling blacklist hits and performing blacklist and whitelist checks.
 */
class Database
{
public:
    /**
     * @brief Checks elements against the blacklist.
     *
     * @param elements The set of elements to check against the blacklist.
     * @return A map indicating whether each element is in the blacklist (true) or not (false).
     */
    virtual std::map<std::string, bool> CheckInBlacklist(const std::unordered_set<std::string> &elements) = 0;

    /**
     * @brief Checks elements against the whitelist.
     *
     * @param elements The set of elements to check against the whitelist.
     * @return A map indicating whether each element is in the whitelist (true) or not (false).
     */
    virtual std::map<std::string, bool> CheckInWhitelist(const std::unordered_set<std::string> &elements) = 0;

    /**
     * @brief Destructor for the Database class.
     */
    virtual ~Database() = default;

private:
    /**
     * @brief Handles a blacklist hit for the given element.
     *
     * @param element The element to handle a blacklist hit for.
     */
    virtual void HandleBlacklistHit(const std::string &element) = 0;
};
