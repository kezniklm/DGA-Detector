#include "Publisher.hpp"

void Publisher::Process()
{
	ValidatedDomains domains;
	while (!cancellation_token.load())
	{
		if (publisher_queue_->try_pop(domains))
		{
			nlohmann::json json_packet = ToJson(domains.domain_return_code_pairs_);

			std::string message_to_send = json_packet.dump(4);

			message_publisher_->PublishMessage(message_to_send);
		}
	}
}

nlohmann::json Publisher::ToJson(const std::unordered_map<std::string, int> &domain_return_code_pairs)
{
	nlohmann::json output_json;
	for (const auto &pair : domain_return_code_pairs)
	{
		// Each domain and its return code is added as a new entry in the JSON object.
		output_json["domains"][pair.first] = pair.second;
	}
	return output_json;
}
