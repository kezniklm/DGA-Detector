/**
 * @file NetworkAnalyserException.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Defines the exception class for handling errors related to the network analyser component.
 *
 * This file declares the exception class NetworkAnalyserException, which is used to handle errors related to the network analyser component in the network traffic monitoring and analysis application.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 */

#pragma once

#include "DetectorException.hpp"

/**
 * @brief Exception class for handling errors related to the network analyser component.
 */
class NetworkAnalyserException final : public DetectorException
{
public:
    /**
     * @brief Constructs a NetworkAnalyserException object with a message and an error code.
     * @param msg The error message.
     * @param c The error code.
     */
    NetworkAnalyserException(const std::string &msg, const int c) : DetectorException(msg, c) {}
};
