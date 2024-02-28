/**
 * @file DNSPacketInfo.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Declaration of the DNSPacketInfo struct for representing DNS packet information.
 *
 * The main components of this file include:
 * - Declaration of the DNSPacketInfo struct, representing information about DNS packets, including domain names and response codes.
 * - Declaration of member variables to store domain names and response codes.
 * - Declaration of constructors for creating DNSPacketInfo objects with provided domain names and response codes.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 *
 */

#pragma once

#include <string>
#include <vector>

/**
 * @struct DNSPacketInfo
 * @brief Represents information about DNS packets.
 */
struct DNSPacketInfo
{
    std::vector<std::string> domain_names;

    int response_code{};

    /**
     * @brief Constructs a DNSPacketInfo object with provided domain names and response code.
     *
     * @param domains Vector of domain names.
     * @param code Response code.
     */
    DNSPacketInfo(const std::vector<std::string> &domains, int code)
        : domain_names(domains), response_code(code) {}

    /**
     * @brief Default constructor for DNSPacketInfo.
     */
    DNSPacketInfo() = default;
};