import argparse
import json
import sys

from src.return_codes import ReturnCodes


class Arguments:
    def __init__(self) -> None:
        self.parser = argparse.ArgumentParser(description="Application configuration")
        self.config = self.__load_config()
        self.__add_arguments()

    def __load_config(self) -> dict:
        """Loads configuration from 'appsettings.json' if available."""
        try:
            with open("appsettings.json", "r") as file:
                return json.load(file)
        except FileNotFoundError:
            print("appsettings.json not found. Using default values.")
        except json.JSONDecodeError:
            print("Error decoding appsettings.json. Ensure it's properly formatted.")
        return {}

    def __add_arguments(self) -> None:
        """Adds command line arguments to the parser with defaults from the config file."""
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
            "-a",
            "--api",
            type=str,
            default=self.config.get("api"),
            required="api" not in self.config,
            help='Domain name for the API, e.g., "example.com". Required if not specified in appsettings.json',
        )

    def parse_args(self) -> argparse.Namespace:
        """Parses the command line arguments and checks for missing arguments."""
        args = self.parser.parse_args()

        missing_args = [name for name, value in vars(args).items() if value is None]
        if missing_args:
            print(f"Error: Missing argument(s): {', '.join(missing_args)}")
            self.parser.print_help()
            sys.exit(ReturnCodes.MISSING_ARGUMENTS.value)

        return args
