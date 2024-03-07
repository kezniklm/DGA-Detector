/**
 * @file Logger.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Declaration of the Logger class for logging messages with different levels of severity.
 *
 * The Logger class encapsulates the Poco logging framework, providing a simple interface for logging messages at various
 * levels of severity, such as debug, information, notice, warning, error, critical, and fatal. It supports logging to different
 * channels based on the operating system, utilizing the EventLogChannel on Windows and SyslogChannel on other platforms.
 *
 * Key features of the Logger class include:
 * - Logging messages with different levels of severity.
 * - Automatic initialization of the appropriate logging channel based on the operating system.
 * - Easy configuration of the log level at runtime.
 *
 * @version 1.0
 * @date 2024-03-07
 * @copyright Copyright (c) 2024
 *
 */

#pragma once

#include <Poco/Logger.h>
#include <Poco/AutoPtr.h>
#ifdef WIN32
#include <Poco/EventLogChannel.h>
#else
#include <Poco/SyslogChannel.h>
#endif

/**
 * @class Logger
 * @brief Handles logging across different severity levels and initializes logging channels based on OS.
 */
class Logger
{
public:
    /**
     * @brief Constructs a Logger object with a specified name.
     *
     * @param name The name of the logger, used for identifying different loggers or log sources.
     */
    Logger(const std::string &name) : _logger(Poco::Logger::get(name))
    {
        Initialize(name);
    }

    /**
     * @brief Logs a message with the "information" severity level.
     *
     * @param message The message to log.
     */
    void log(const std::string &message)
    {
        std::cerr << message << '\n';
        _logger.information(message);
    }

    /**
     * @brief Logs a message with the "debug" severity level.
     *
     * @param message The message to log.
     */
    void debug(const std::string &message)
    {
        std::cerr << message << '\n';
        _logger.debug(message);
    }

    /**
     * @brief Logs a message with the "information" severity level.
     *
     * @param message The message to log.
     */
    void information(const std::string &message)
    {
        log(message);
    }

    /**
     * @brief Logs a message with the "notice" severity level.
     *
     * @param message The message to log.
     */
    void notice(const std::string &message)
    {
        std::cerr << message << '\n';
        _logger.notice(message);
    }

    /**
     * @brief Logs a message with the "warning" severity level.
     *
     * @param message The message to log.
     */
    void warning(const std::string &message)
    {
        std::cerr << message << '\n';
        _logger.warning(message);
    }

    /**
     * @brief Logs a message with the "error" severity level.
     *
     * @param message The message to log.
     */
    void error(const std::string &message)
    {
        std::cerr << message << '\n';
        _logger.error(message);
    }

    /**
     * @brief Logs a message with the "critical" severity level.
     *
     * @param message The message to log.
     */
    void critical(const std::string &message)
    {
        std::cerr << message << '\n';
        _logger.critical(message);
    }

    /**
     * @brief Logs a message with the "fatal" severity level.
     *
     * @param message The message to log.
     */
    void fatal(const std::string &message)
    {
        std::cerr << message << '\n';
        _logger.fatal(message);
    }

    /**
     * @brief Sets the log level of the logger.
     *
     * @param priority The priority level to set.
     */
    void setLogLevel(Poco::Message::Priority priority)
    {
        _logger.setLevel(priority);
    }

private:
    /** Reference to the internal Poco::Logger instance. */
    Poco::Logger &_logger;

    /**
     * @brief Initializes the logger with an appropriate channel based on the operating system.
     *
     * @param name The name of the logger.
     */
    void Initialize(const std::string &name)
    {
        Poco::AutoPtr<Poco::Channel> channel;
#ifdef WIN32
        channel = new Poco::EventLogChannel(name);
#else
        channel = new Poco::SyslogChannel(name);
#endif
        _logger.setChannel(channel);
        _logger.setLevel(Poco::Message::PRIO_TRACE);
    }
};