#pragma once

#include <iostream>
#include <string>
#include <memory>
#include <vector>

#include <bsl_memory.h>
#include <bsl_optional.h>
#include <bsl_vector.h>

#include <rmqa_connectionstring.h>
#include <rmqa_producer.h>
#include <rmqa_rabbitcontext.h>
#include <rmqa_topology.h>
#include <rmqa_vhost.h>
#include <rmqp_producer.h>
#include <rmqt_confirmresponse.h>
#include <rmqt_exchange.h>
#include <rmqt_message.h>
#include <rmqt_result.h>
#include <rmqt_vhostinfo.h>

#include "Exceptions.hpp"
#include "ReturnCodes.hpp"

using namespace BloombergLP;

extern std::atomic<bool> cancellation_token;

class MessagePublisher
{
public:
	MessagePublisher(const std::string &connection_string, const std::string &queue_name)
		: queue_name_(queue_name)
	{
		InitializeProducer(connection_string);
	}

	void PublishMessage(const std::string &message)
	{
		rmqt::Message rmq_message(bsl::make_shared<bsl::vector<uint8_t>>(message.begin(), message.end()));

		producer_->send(
			rmq_message,
			"",
			[this](const rmqt::Message &message, const bsl::string &routingKey, const rmqt::ConfirmResponse &response)
			{
				HandleSendResponse(message, routingKey, response);
			});

		WaitForConfirmation();
	}

private:
	std::string queue_name_;
	bsl::shared_ptr<rmqa::Producer> producer_;

	void InitializeProducer(const std::string &connection_string)
	{
		rmqa::RabbitContext rabbit;
		bsl::optional<rmqt::VHostInfo> vhost_info = rmqa::ConnectionString::parse(connection_string);

		if (!vhost_info)
		{
			throw MessagePublisherException("Failed to parse connection string: " + connection_string, MESSAGE_PUBLISHER_CREATION_FAILURE);
		}

		bsl::shared_ptr<rmqa::VHost> vhost = rabbit.createVHostConnection("Detector", vhost_info.value());

		const uint16_t max_outstanding_confirms = 10;
		rmqa::Topology topology;
		rmqt::ExchangeHandle exchange = topology.defaultExchange();
		rmqt::QueueHandle queue = topology.addQueue(queue_name_);
		topology.bind(exchange, queue, queue_name_);

		rmqt::Result<rmqa::Producer> producer_result = vhost->createProducer(topology, exchange, max_outstanding_confirms);

		if (!producer_result)
		{
			throw MessagePublisherException("Error creating connection: " + producer_result.error(), MESSAGE_PUBLISHER_CREATION_FAILURE);
		}

		producer_ = producer_result.value();
	}

	void HandleSendResponse(const rmqt::Message &message, const bsl::string &routingKey, const rmqt::ConfirmResponse &response)
	{
		if (response.status() == rmqt::ConfirmResponse::ACK)
		{
			std::cout << "Message acknowledged successfully: " << message.guid() << "\n";
		}
		else
		{
			std::cerr << "Message not acknowledged: " << message.guid() << " for routing key " << routingKey << "\n";
		}
	}

	void WaitForConfirmation()
	{
		constexpr int NUMBER_OF_SECONDS = 5;

		BloombergLP::bsls::TimeInterval timeout(NUMBER_OF_SECONDS);

		if (!producer_->waitForConfirms(timeout))
		{
			throw MessagePublisherException("Message timeouted", MESSAGE_PUBLISHER_TIMEOUT);
		}
	}
};
