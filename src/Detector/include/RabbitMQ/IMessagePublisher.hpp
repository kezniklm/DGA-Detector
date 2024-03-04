/**
 * @file IMessagePublisher.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Declaration of the IMessagePublisher interface for message publishing operations.
 *
 * The IMessagePublisher interface defines a standard for implementing message publishing capabilities
 * across different messaging services. It encapsulates the essential operation needed for publishing messages,
 * providing a unified approach to message distribution. This interface is crucial for applications that
 * require decoupling of message sending operations from specific messaging service implementations, thereby
 * facilitating easier integration and testing.
 *
 * Key functionalities include:
 * - Publishing messages to a designated messaging service.
 *
 * @version 1.0
 * @date 2024-03-04
 * @copyright Copyright (c) 2024
 *
 */

#pragma once

#include <string>

class IMessagePublisher
{
public:
    /**
     * @brief Virtual destructor to ensure proper cleanup of derived classes.
     */
    virtual ~IMessagePublisher() = default;

    /**
     * @brief Publishes a message to a messaging service.
     *
     * This pure virtual function must be implemented by derived classes to handle the
     * specifics of message publishing, accommodating various messaging protocols and services.
     *
     * @param message The message to be published, encapsulated as a std::string.
     */
    virtual void PublishMessage(const std::string &message) const = 0;
};
