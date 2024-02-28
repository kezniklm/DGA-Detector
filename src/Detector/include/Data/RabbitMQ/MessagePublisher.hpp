#pragma once

#include <iostream>
#include <string>
#include <amqp_tcp_socket.h>
#include <amqp.h>
#include <amqp_framing.h>

class MessagePublisher
{
public:
	MessagePublisher(const std::string &connectionString, const std::string &queueName)
	{
		parseConnectionString(connectionString);
		initialize();
		queueName_ = queueName;
	}

	~MessagePublisher()
	{
		cleanup();
	}

	void PublishMessage(const std::string &message)
	{
		amqp_bytes_t message_bytes = amqp_cstring_bytes(message.c_str());
		amqp_basic_properties_t props;
		props._flags = AMQP_BASIC_CONTENT_TYPE_FLAG | AMQP_BASIC_DELIVERY_MODE_FLAG;
		props.content_type = amqp_cstring_bytes("text/plain");
		props.delivery_mode = 2; // persistent delivery mode

		amqp_basic_publish(conn, 1, amqp_empty_bytes, amqp_cstring_bytes(queueName_.c_str()), 0, 0, &props, message_bytes);
	}

private:
	std::string hostname;
	int port;
	std::string username;
	std::string password;
	std::string virtualHost;
	std::string queueName_;

	amqp_connection_state_t conn;
	amqp_socket_t *socket;

	void parseConnectionString(const std::string &connectionString)
	{
		std::string prefix = "amqp://";
		size_t prefixPos = connectionString.find(prefix);
		if (prefixPos == std::string::npos)
		{
			throw std::invalid_argument("Invalid connection string format");
		}

		size_t userInfoEndPos = connectionString.find('@');
		if (userInfoEndPos == std::string::npos)
		{
			throw std::invalid_argument("Invalid connection string format: Missing user info");
		}

		size_t userInfoStartPos = prefixPos + prefix.length();
		std::string userInfo = connectionString.substr(userInfoStartPos, userInfoEndPos - userInfoStartPos);
		size_t colonPos = userInfo.find(':');
		if (colonPos == std::string::npos)
		{
			throw std::invalid_argument("Invalid connection string format: Missing password");
		}

		username = userInfo.substr(0, colonPos);
		password = userInfo.substr(colonPos + 1);

		size_t hostStartPos = userInfoEndPos + 1;
		size_t virtualHostStartPos = connectionString.find('/', hostStartPos);
		if (virtualHostStartPos == std::string::npos)
		{
			throw std::invalid_argument("Invalid connection string format: Missing virtual host");
		}

		std::string hostPart = connectionString.substr(hostStartPos, virtualHostStartPos - hostStartPos);
		size_t colonHostPos = hostPart.find(':');
		if (colonHostPos != std::string::npos)
		{
			hostname = hostPart.substr(0, colonHostPos);
			std::string portStr = hostPart.substr(colonHostPos + 1);
			port = std::stoi(portStr); // Only parse port if it exists
		}
		else
		{
			hostname = hostPart;
			port = 5672; // Default AMQP port
		}

		virtualHost = connectionString.substr(virtualHostStartPos + 1);
		if (virtualHost == "%2F")
		{
			virtualHost = "/";
		}
	}

	void initialize()
	{
		conn = amqp_new_connection();

		socket = amqp_tcp_socket_new(conn);
		if (!socket)
		{
			throw std::runtime_error("Failed to create TCP socket");
		}

		int status = amqp_socket_open(socket, hostname.c_str(), port);
		if (status)
		{
			throw std::runtime_error("Failed to open TCP socket");
		}

		amqp_maybe_release_buffers(conn);

		amqp_login(conn, virtualHost.c_str(), 0, 131072, 0, AMQP_SASL_METHOD_PLAIN, username.c_str(), password.c_str());

		amqp_channel_open(conn, 1);
		amqp_get_rpc_reply(conn);
	}

	void cleanup()
	{
		amqp_channel_close(conn, 1, AMQP_REPLY_SUCCESS);
		amqp_connection_close(conn, AMQP_REPLY_SUCCESS);
		amqp_destroy_connection(conn);
	}
};
