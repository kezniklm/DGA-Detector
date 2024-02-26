#include "Publisher.hpp"

void Publisher::Process() const
{
	DNSPacketInfo packet_info;
	while (!cancellation_token.load())
	{
		if (publisher_queue_->try_pop(packet_info))
		{
			nlohmann::json json_packet = ToJson(packet_info);

			std::string message_to_send = json_packet.dump(4);

			message_publisher_->PublishMessage(message_to_send);
		}
	}
}

nlohmann::json Publisher::ToJson(const DNSPacketInfo &packet_info)
{
	nlohmann::json j;
	j["domain_names"] = packet_info.domain_names;
	j["response_code"] = packet_info.response_code;
	return j;
}
