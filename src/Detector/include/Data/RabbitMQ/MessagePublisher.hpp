#pragma once

#include <SimpleAmqpClient/SimpleAmqpClient.h>
#include <iostream>
#include <string>
#include <thread> // For std::this_thread::sleep_for
#include <chrono> // For std::chrono::seconds
#include <utility>
#include "Exceptions.hpp"
#include "ReturnCodes.hpp"

class MessagePublisher
{
public:
	MessagePublisher(const std::string &connection_string, std::string queue_name)
		: queue_name_(std::move(queue_name))
	{
		try
		{
			channel_ = AmqpClient::Channel::CreateFromUri(connection_string);
			DeclareQueue();
		}
		catch (const std::exception &e)
		{
			throw MessagePublisherException(e.what(), MESSAGE_PUBLISHER_CREATION_FAILURE);
		}
	}

	bool PublishMessage(const std::string &message, int retry_count = 5, int retry_delay_seconds = 2)
	{
		int attempts = 0;
		while (attempts < retry_count)
		{
			try
			{
				// Prepare a message to be sent
				AmqpClient::BasicMessage::ptr_t outgoing_message = AmqpClient::BasicMessage::Create(message);

				// Mark the message as persistent
				outgoing_message->DeliveryMode(AmqpClient::BasicMessage::dm_persistent);

				// Publish the message to the default exchange with the routing key as the queue name
				channel_->BasicPublish("", queue_name_, outgoing_message);

				std::cout << "Sent: " << message << std::endl;
				return true; // Message sent successfully
			}
			catch (const std::exception &e)
			{
				std::cerr << "Failed to publish message, attempt " << (attempts + 1) << ": " << e.what() << std::endl;
				++attempts;
				// Wait before retrying
				std::this_thread::sleep_for(std::chrono::seconds(retry_delay_seconds));
			}
		}
		std::cerr << "Failed to publish message after " << retry_count << " attempts." << std::endl;
		return false; // Message sending failed after retries
	}

private:
	std::string queue_name_;
	AmqpClient::Channel::ptr_t channel_;

	void DeclareQueue()
	{
		bool durable = true;
		channel_->DeclareQueue(queue_name_, durable, false, false, false);
	}
};
