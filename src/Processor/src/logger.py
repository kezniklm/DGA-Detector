"""
 * @file logger.py
 * @brief Provides a class for creating and configuring a logger with both console and OS-specific logging capabilities.
 *
 * This file contains the implementation of the Logger class, which facilitates the creation and configuration of a logger for an application. The logger is configured to support both console logging and OS-specific logging (Windows event log or Unix/Linux syslog).
 *
 * The main functionalities of this file include:
 * - Initializing the Logger instance with an optional application name.
 * - Configuring logging to include console logging and OS-specific logging.
 * - Configuring console logging with a DEBUG level and a standard formatter.
 * - Configuring OS-specific logging for Windows or Unix/Linux systems.
 * - Configuring Windows-specific event log logging with an ERROR level.
 * - Configuring Unix/Linux-specific syslog logging with an ERROR level.
 * - Creating and returning a standard logging formatter.
 * - Providing a method to retrieve the configured logger instance.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
 """

import logging
import platform


class Logger:
    """A class for creating and configuring a logger with both console and OS-specific logging capabilities."""

    def __init__(self, app_name="DGA-Detector"):
        """Initialize the Logger instance.
        Args:
            app_name (str): The name of the application for which logging is set up. Defaults to 'DGA-Detector'.
        """
        self.logger = logging.getLogger(app_name)
        self.configure_logging()

    def clear_handlers(self):
        """Clear all existing handlers from the logger."""
        for handler in self.logger.handlers[
            :
        ]:  # Iterate over a copy of the handler list
            self.logger.removeHandler(handler)

    def configure_logging(self):
        """Configure logging to include console logging and OS-specific logging."""
        self.clear_handlers()  # Clear existing handlers before adding new ones
        self.logger.setLevel(logging.INFO)
        self.configure_console_logging()
        self.configure_os_specific_logging()

    def configure_console_logging(self):
        """Configure console logging with DEBUG level and a standard formatter."""
        console_handler = logging.StreamHandler()
        console_handler.setLevel(logging.DEBUG)
        formatter = self.get_standard_formatter()
        console_handler.setFormatter(formatter)
        self.logger.addHandler(console_handler)

    def configure_os_specific_logging(self):
        """Configure OS-specific logging for Windows or Unix/Linux systems."""
        formatter = self.get_standard_formatter()

        if platform.system() == "Windows":
            self.configure_windows_logging(formatter)
        else:
            self.configure_unix_logging(formatter)

    def configure_windows_logging(self, formatter):
        """Configure Windows-specific event log logging.
        Args:
            formatter (logging.Formatter): The logging formatter to use.
        """
        from logging.handlers import NTEventLogHandler

        event_log_handler = NTEventLogHandler(
            appname="DGA-Detector", logtype="Application"
        )
        event_log_handler.setLevel(logging.ERROR)
        event_log_handler.setFormatter(formatter)
        self.logger.addHandler(event_log_handler)

    def configure_unix_logging(self, formatter):
        """Configure Unix/Linux-specific syslog logging.
        Args:
            formatter (logging.Formatter): The logging formatter to use.
        """
        from logging.handlers import SysLogHandler

        syslog_handler = SysLogHandler(address="/dev/log")
        syslog_handler.setLevel(logging.ERROR)
        syslog_handler.setFormatter(formatter)
        self.logger.addHandler(syslog_handler)

    def get_standard_formatter(self):
        """Create and return a standard logging formatter.
        Returns:
            logging.Formatter: The configured formatter for log messages.
        """
        return logging.Formatter("%(asctime)s - %(name)s - %(levelname)s - %(message)s")

    def get_logger(self):
        """Get the configured logger instance.
        Returns:
            logging.Logger: The configured logger.
        """
        return self.logger
