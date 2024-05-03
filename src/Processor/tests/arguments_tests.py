"""
 * @file arguments_tests.py
 * @brief Unit tests for the Arguments class.
 *
 * This file contains unit tests for the Arguments class, which is responsible for handling program arguments and configurations.
 * The tests cover various scenarios including reading configurations from appsettings.json, command line argument overrides, default values usage, missing required arguments, and handling unexpected arguments.
 *
 * Main functionalities of this file include:
 * - Testing behavior when appsettings.json is missing, complete, or contains errors.
 * - Testing command line argument overrides for configurations.
 * - Verifying the usage of default values.
 * - Checking the handling of missing required arguments.
 * - Ensuring all required arguments are provided via command line override.
 * - Testing the parser's graceful handling of unexpected arguments.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

import json
import sys
import unittest
from unittest.mock import mock_open, patch

sys.path.append("..")

from src.utils.arguments import Arguments
from src.utils.return_codes import ReturnCodes


class ArgumentsTests(unittest.TestCase):
    """A set of test cases for the Arguments class."""

    def setUp(self):
        """Set up test environment."""
        self.logger_mock = patch("src.logging.logger.Logger").start()
        self.addCleanup(patch.stopall)
        self.default_args = {
            "rabbitmq": "user:password@host:port/vhost",
            "queue": "task_queue",
            "database": "user:password@host:port/dbname",
            "dbname": "test_db",
            "processes": 4,
            "queue_size": 10,
        }
        self.appsettings_json = json.dumps(self.default_args)

    def test_missing_appsettings(self):
        """Test behavior when appsettings.json is missing."""
        with patch("builtins.open", mock_open(read_data="{}")) as mock_file:
            mock_file.side_effect = FileNotFoundError
            args = Arguments(self.logger_mock)
            self.assertEqual(args.config, {})
            self.logger_mock.error.assert_called_once_with(
                "appsettings.json not found. Using default values."
            )

    def test_complete_appsettings(self):
        """Test behavior with complete appsettings.json."""
        with patch("builtins.open", mock_open(read_data=self.appsettings_json)):
            args = Arguments(self.logger_mock)
            self.assertEqual(args.config, self.default_args)

    def test_error_in_appsettings(self):
        """Test behavior when appsettings.json contains errors."""
        with patch("builtins.open", mock_open(read_data="{not json")):
            args = Arguments(self.logger_mock)
            self.logger_mock.error.assert_called_with(
                "Error decoding appsettings.json. Ensure it's properly formatted."
            )

    def test_command_line_arguments_override(self):
        """Test behavior when command line arguments override settings."""
        test_args = ["program", "-r", "new_rabbitmq", "-q", "new_queue"]
        with patch("builtins.open", mock_open(read_data=self.appsettings_json)), patch(
            "sys.argv", test_args
        ):
            args = Arguments(self.logger_mock)
            parsed_args = args.parse_args()
            self.assertEqual(parsed_args.rabbitmq, "new_rabbitmq")
            self.assertEqual(parsed_args.queue, "new_queue")
            self.assertEqual(parsed_args.database, self.default_args["database"])

    def test_default_values_used(self):
        """Test behavior when default values are used."""
        with patch("builtins.open", mock_open(read_data=self.appsettings_json)):
            args = Arguments(self.logger_mock)
            parsed_args = args.parse_args()
            self.assertEqual(parsed_args.database, self.default_args["database"])

    def test_required_arguments_missing(self):
        """Test behavior when required arguments are missing."""
        with patch("builtins.open", mock_open(read_data="{}")), patch(
            "sys.exit"
        ) as mock_exit:
            args = Arguments(self.logger_mock)
            args.parse_args()
            mock_exit.assert_called_with(ReturnCodes.MISSING_ARGUMENTS.value)

    def test_all_required_arguments_provided(self):
        """Test behavior when all required arguments are provided."""
        test_args = [
            "program",
            "-r",
            "new_rabbitmq",
            "-q",
            "new_queue",
            "-d",
            "new_database",
            "-n",
            "new_dbname",
        ]
        with patch("builtins.open", mock_open(read_data=self.appsettings_json)), patch(
            "sys.argv", test_args
        ):
            args = Arguments(self.logger_mock)
            parsed_args = args.parse_args()
            self.assertEqual(parsed_args.database, "new_database")
            self.assertEqual(parsed_args.dbname, "new_dbname")

    def test_unexpected_arguments(self):
        """Test behavior when unexpected arguments are provided."""
        test_args = ["program", "--unexpected", "value"]
        with patch("builtins.open", mock_open(read_data=self.appsettings_json)), patch(
            "sys.argv", test_args
        ), patch("sys.exit") as mock_exit:
            args = Arguments(self.logger_mock)
            args.parse_args()
            mock_exit.assert_called_once()
