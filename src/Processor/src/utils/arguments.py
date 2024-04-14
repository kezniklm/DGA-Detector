"""
 * @file Arguments.py
 * @brief Handles parsing and storing of command line arguments for the application.
 *
 * This file contains the implementation of the Arguments class, which is responsible for parsing command-line arguments using the argparse library. It also loads configuration settings from a JSON file named 'appsettings.json' if available.
 *
 * The main functionalities of this file include:
 * - Parsing command-line arguments using the argparse library.
 * - Loading configuration settings from 'appsettings.json'.
 * - Adding command-line arguments to the parser with defaults from the config file.
 * - Checking for missing arguments and printing help message if any are missing.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

import argparse
import json
import sys
from typing import Any, Dict
from multiprocessing import cpu_count

from src.logging.logger import Logger
from src.utils.return_codes import ReturnCodes


class Arguments:
    """
    Handles parsing and storing of command line arguments for the application.

    This class parses command-line arguments using the argparse library and utilizes a logger for logging. It also loads configuration settings from a JSON file named 'appsettings.json' if available.

    Attributes:
        parser (argparse.ArgumentParser): An instance of the ArgumentParser class for defining command-line arguments.
        config (dict): A dictionary containing configuration settings loaded from 'appsettings.json'.
        logger: The logger instance used for logging errors and other messages.
    """

    def __init__(self, logger: Logger) -> None:
        """
        Initialize the Arguments class.

        Initializes the ArgumentParser, loads configuration settings, and sets up the logger for logging.

        Parameters:
            logger: The logger instance used for logging errors and other messages.
        """
        self.logger: Logger = logger
        self.parser: argparse.ArgumentParser = argparse.ArgumentParser(
            description="Application configuration"
        )
        self.config: Dict[str, Any] = self.__load_config()
        self.__add_arguments()

    def __load_config(self) -> dict:
        """
        Loads configuration from 'appsettings.json' if available.

        Returns:
            dict: A dictionary containing configuration settings loaded from 'appsettings.json'.
        """
        try:
            with open("appsettings.json", "r") as file:
                return json.load(file)
        except FileNotFoundError:
            self.logger.error("appsettings.json not found. Using default values.")
        except json.JSONDecodeError:
            self.logger.error(
                "Error decoding appsettings.json. Ensure it's properly formatted."
            )
        return {}

    def __add_arguments(self) -> None:
        """
        Adds command line arguments to the parser with defaults from the config file.
        """
        self.parser.add_argument(
            "-r",
            "--rabbitmq",
            type=str,
            default=self.config.get("rabbitmq"),
            required="rabbitmq" not in self.config,
            help="Connection string for RabbitMQ, formatted as: user:password@host:port/vhost. Required if not specified in appsettings.json",
        )

        self.parser.add_argument(
            "-q",
            "--queue",
            type=str,
            default=self.config.get("queue"),
            required="queue" not in self.config,
            help="Name of the RabbitMQ queue. Required if not specified in appsettings.json",
        )

        self.parser.add_argument(
            "-d",
            "--database",
            type=str,
            default=self.config.get("database"),
            required="database" not in self.config,
            help="Connection string to the database, e.g., 'user:password@host:port/dbname'. Required if not specified in appsettings.json",
        )

        self.parser.add_argument(
            "-n",
            "--dbname",
            type=str,
            default=self.config.get("dbname"),
            required="dbname" not in self.config,
            help="Name of the database. Required if not specified in appsettings.json",
        )

        self.parser.add_argument(
            "-p",
            "--processes",
            type=int,
            default=self.config.get("processes", cpu_count),
            help="Number of processes that extract features. Default is number of cores of device you are using, if not specified in appsettings.json or command line.",
        )

        self.parser.add_argument(
            "-s",
            "--queue-size",
            type=int,
            default=self.config.get("queue_size", 10),
            help="Size of the queue between the RabbitMQ consumer and the extractor. Default is 10 if not specified in appsettings.json or command line.",
        )

    def parse_args(self) -> argparse.Namespace:
        """
        Parses the command line arguments and checks for missing arguments.

        Returns:
            argparse.Namespace: An object containing parsed arguments.
        """
        args = self.parser.parse_args()

        missing_args = [name for name, value in vars(args).items() if value is None]
        if missing_args:
            self.logger.error(f"Error: Missing argument(s): {', '.join(missing_args)}")
            self.parser.print_help()
            sys.exit(ReturnCodes.MISSING_ARGUMENTS.value)

        return args
