#pragma once

#include <iostream>
#include <pcap.h>

#include "MPMCQueue.hpp"

#include "Exceptions.hpp"
#include "Packet.hpp"
#include "ReturnCodes.hpp"

class NetworkAnalyser
{
public:
	explicit NetworkAnalyser(const std::string &device,
							 int buffer_size,
							 rigtorp::MPMCQueue<Packet> *packet_queue);

	~NetworkAnalyser();

	void StartCapture() const;

	void StopCapture() const;

private:
	pcap_t *handle_;

	rigtorp::MPMCQueue<Packet> *queue_;

	void TryToCreateHandle(const char *device);

	void TryToSetBufferSize(int buffer_size) const;

	void TryToActivateHandle() const;

	void TryToSetImmediateMode() const;

	void TryToSetBpfFilter() const;

	void SetSnaplen() const;

	void SetPromiscuousMode() const;

	void SetTimeout() const;
};
