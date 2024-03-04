/**
 * @file ArgumentException.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Defines the exception class for handling invalid command-line arguments.
 *
 * This file declares the exception class ArgumentException, which is used to handle errors related to invalid command-line arguments in the network traffic monitoring and analysis application.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 */

#pragma once

#include "DetectorException.hpp"

/**
 * @brief Exception class for handling invalid command-line arguments.
 */
class ArgumentException final : public DetectorException
{
public:
    /**
     * @brief Constructs an ArgumentException object with a message and an error code.
     * @param msg The error message.
     * @param c The error code.
     */
    ArgumentException(const std::string &msg, const int c) : DetectorException(msg, c) {}
};
