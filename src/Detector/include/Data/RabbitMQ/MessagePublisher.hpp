/**
 * @file MessagePublisher.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Declaration of the MessagePublisher class for publishing messages to an AMQP queue.
 *
 * The main components of this file include:
 * - Declaration of the MessagePublisher class, responsible for publishing messages to an AMQP queue.
 * - Declaration of member variables to store AMQP connection details such as hostname, port, username, password, virtual host, and queue name.
 * - Declaration of member functions for parsing the connection string, initializing the AMQP connection, publishing messages, and cleaning up resources.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 * 
 */

#pragma once

#include <iostream>
#include <string>
#include <string_view>

#include "amqp.h"
#include "amqp_framing.h"
#include "amqp_tcp_socket.h"
#include "IMessagePublisher.hpp"

/**
 * @class MessagePublisher
 * @brief Publishes messages to an AMQP queue.
 */
class MessagePublisher : public IMessagePublisher
{
public:
    /**
     * @brief Constructs a MessagePublisher object.
     *
     * @param connection_string The AMQP connection string.
     * @param queue_name The name of the AMQP queue to publish messages to.
     */
    MessagePublisher(const std::string &connection_string, const std::string &queue_name)
    {
        ParseConnectionString(connection_string);
        Initialize();
        queue_name_ = queue_name;
    }

    /**
     * @brief Destroys the MessagePublisher object and cleans up resources.
     */
    ~MessagePublisher()
    {
        Cleanup();
    }

    /**
     * @brief Publishes a message to the AMQP queue.
     *
     * @param message The message to publish.
     */
    void PublishMessage(const std::string &message) const
    {
        const amqp_bytes_t message_bytes = amqp_cstring_bytes(message.c_str());
        amqp_basic_properties_t props;
        props._flags = AMQP_BASIC_CONTENT_TYPE_FLAG | AMQP_BASIC_DELIVERY_MODE_FLAG;
        props.content_type = amqp_cstring_bytes("text/plain");
        props.delivery_mode = 2; // persistent delivery mode

        amqp_basic_publish(conn_,
                           1,
                           amqp_empty_bytes,
                           amqp_cstring_bytes(queue_name_.c_str()),
                           0,
                           0,
                           &props,
                           message_bytes);
    }

private:
    /**
     * @brief Parses the connection string to extract connection details.
     *
     * @param connection_string The AMQP connection string.
     * @throw std::invalid_argument if the connection string is invalid.
     */
    void ParseConnectionString(const std::string &connection_string)
    {
        constexpr std::string_view prefix = "amqp://";
        const size_t prefix_pos = connection_string.find(prefix);
        if (prefix_pos == std::string::npos)
        {
            throw std::invalid_argument("Invalid connection string format");
        }

        const size_t user_info_end_pos = connection_string.find('@');
        if (user_info_end_pos == std::string::npos)
        {
            throw std::invalid_argument("Invalid connection string format: Missing user info");
        }

        const size_t user_info_start_pos = prefix_pos + prefix.length();
        std::string user_info = connection_string.substr(user_info_start_pos, user_info_end_pos - user_info_start_pos);
        const size_t colon_pos = user_info.find(':');
        if (colon_pos == std::string::npos)
        {
            throw std::invalid_argument("Invalid connection string format: Missing password_");
        }

        username_ = user_info.substr(0, colon_pos);
        password_ = user_info.substr(colon_pos + 1);

        const size_t host_start_pos = user_info_end_pos + 1;
        const size_t virtual_host_start_pos = connection_string.find('/', host_start_pos);
        if (virtual_host_start_pos == std::string::npos)
        {
            throw std::invalid_argument("Invalid connection string format: Missing virtual host");
        }

        std::string host_part = connection_string.substr(host_start_pos, virtual_host_start_pos - host_start_pos);
        const size_t colon_host_pos = host_part.find(':');
        if (colon_host_pos != std::string::npos)
        {
            hostname_ = host_part.substr(0, colon_host_pos);
            const std::string port_str = host_part.substr(colon_host_pos + 1);
            port_ = std::stoi(port_str); // Only parse port_ if it exists
        }
        else
        {
            hostname_ = host_part;
            port_ = 5672; // Default AMQP port_
        }

        virtual_host_ = connection_string.substr(virtual_host_start_pos + 1);
        if (virtual_host_ == "%2F")
        {
            virtual_host_ = "/";
        }
    }

    /**
     * @brief Initializes the AMQP connection and channel.
     *
     * @throw std::runtime_error if initialization fails.
     */
    void Initialize()
    {
        conn_ = amqp_new_connection();

        socket_ = amqp_tcp_socket_new(conn_);
        if (!socket_)
        {
            throw std::runtime_error("Failed to create TCP socket_");
        }

        const int status = amqp_socket_open(socket_, hostname_.c_str(), port_);
        if (status)
        {
            throw std::runtime_error("Failed to open TCP socket_");
        }

        amqp_maybe_release_buffers(conn_);

        amqp_login(conn_,
                   virtual_host_.c_str(),
                   0,
                   131072,
                   0,
                   AMQP_SASL_METHOD_PLAIN,
                   username_.c_str(),
                   password_.c_str());

        amqp_channel_open(conn_, 1);
        amqp_get_rpc_reply(conn_);
    }

    /**
     * @brief Cleans up the AMQP connection and channel.
     */
    void Cleanup() const
    {
        amqp_channel_close(conn_, 1, AMQP_REPLY_SUCCESS);
        amqp_connection_close(conn_, AMQP_REPLY_SUCCESS);
        amqp_destroy_connection(conn_);
    }

    /** Hostname of the AMQP server */
    std::string hostname_;

    /** Port of the AMQP server */
    int port_;

    /** Username for authenticating with the AMQP server */
    std::string username_;

    /** Password for authenticating with the AMQP server */
    std::string password_;

    /** Virtual host for the AMQP connection */
    std::string virtual_host_;

    /** Name of the AMQP queue */
    std::string queue_name_;

    /** AMQP connection state */
    amqp_connection_state_t conn_{};

    /** AMQP socket */
    amqp_socket_t *socket_{};
};
