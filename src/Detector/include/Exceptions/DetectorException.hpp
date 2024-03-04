/**
 * @file DetectorException.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Defines the base exception class for the network traffic monitoring and analysis application.
 *
 * This file declares the base exception class DetectorException, which serves as the parent class for all custom exceptions used in the application.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 */

#pragma once

#include <exception>

/**
 * @brief Base class for exceptions specific to the Detector application.
 */
class DetectorException : public std::exception
{
protected:
    std::string message_; /**< The error message associated with the exception. */
    int code_;            /**< The error code associated with the exception. */

public:
    /**
     * @brief Constructs a DetectorException object with a message and an error code.
     * @param msg The error message.
     * @param c The error code.
     */
    DetectorException(std::string msg, const int c) : message_(std::move(msg)), code_(c) {}

    /**
     * @brief Retrieves the error code associated with the exception.
     * @return The error code.
     */
    int GetCode() const noexcept { return code_; }

    /**
     * @brief Retrieves the error message associated with the exception.
     * @return The error message.
     */
    const char *what() const noexcept override { return message_.c_str(); }
};
