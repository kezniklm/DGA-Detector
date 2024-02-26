#pragma once 

#include <amqpcpp.h>
#include <amqpcpp/libboostasio.h>
#include <boost/asio.hpp>
#include <iostream>
#include <string>
#include <thread>
#include <chrono>

class MessagePublisher
{
public:
	MessagePublisher(const std::string &address, const std::string &queueName)
		: ioService_(), handler_(ioService_), connection_(&handler_, AMQP::Address(address)), channel_(&connection_), queueName_(queueName)
	{
		try
		{
			// Declare the queue
			channel_.declareQueue(queueName, AMQP::durable);
		}
		catch (const std::exception &e)
		{
			throw std::runtime_error(std::string("Failed to declare queue: ") + e.what());
		}
	}

	bool PublishMessage(const std::string &message, int retryCount = 5, int retryDelaySeconds = 2)
	{
		int attempts = 0;
		while (attempts < retryCount)
		{
			try
			{
				// Publish message to the queue
				channel_.startTransaction();
				channel_.publish("", queueName_, message);
				channel_.commitTransaction().onSuccess([&]()
													   { std::cout << "Message published successfully: " << message << std::endl; })
					.onError([&](const char *message)
							 { throw std::runtime_error(message); });

				return true; // Message sent successfully
			}
			catch (const std::exception &e)
			{
				std::cerr << "Failed to publish message, attempt " << (attempts + 1) << ": " << e.what() << std::endl;
				++attempts;
				// Wait before retrying
				std::this_thread::sleep_for(std::chrono::seconds(retryDelaySeconds));
			}
		}

		std::cerr << "Failed to publish message after " << retryCount << " attempts." << std::endl;
		return false; // Message sending failed after retries
	}

private:
	boost::asio::io_service ioService_;
	AMQP::LibBoostAsioHandler handler_;
	AMQP::TcpConnection connection_;
	AMQP::TcpChannel channel_;
	std::string queueName_;
};