#pragma once

#include <exception>
#include <iostream>

class DetectorException : public std::exception
{
protected:
	std::string message_;
	int code_;

public:
	DetectorException(std::string msg, const int c) : message_(std::move(msg)), code_(c)
	{
	}

	int get_code() const noexcept
	{
		return code_;
	}

	const char* what() const noexcept override
	{
		return message_.c_str();
	}
};

class ArgumentException final : public DetectorException
{
public:
	ArgumentException(const std::string& msg, const int c) : DetectorException(msg, c)
	{
	}
};

class NetworkAnalyserException final : public DetectorException
{
public:
	NetworkAnalyserException(const std::string& msg, const int c) : DetectorException(msg, c)
	{
	}
};
