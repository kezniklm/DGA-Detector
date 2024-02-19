#pragma once

#include <iostream>
#include <exception>

class DetectorException : public std::exception
{
protected: // Change from private to protected to allow access from derived classes
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

	// Override what() from std::exception
	const char* what() const noexcept override
	{
		return message_.c_str();
	}
};

class ArgumentException : public DetectorException
{
public:
	ArgumentException(const std::string& msg, const int c) : DetectorException(msg, c)
	{
	}
};

class NetworkAnalyserException : public DetectorException
{
public:
	NetworkAnalyserException(const std::string& msg, const int c) : DetectorException(msg, c)
	{
	}
};
