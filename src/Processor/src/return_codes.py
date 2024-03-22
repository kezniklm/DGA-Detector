"""
 * @file return_codes.py
 * @brief Defines return codes for the application.
 *
 * This file contains the definition of the ReturnCodes enumeration, which represents various return codes that the application may encounter during its execution. It includes codes for indicating success and different types of errors.
 *
 * The main functionalities of this file include:
 * - Defining an enumeration to represent return codes.
 * - Enumerating different types of errors and their meanings.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

from enum import Enum, auto


class ReturnCodes(Enum):
    """
    Enumeration representing return codes for the application.

    This enumeration defines various return codes that the application may encounter during its execution.

    Attributes:
        SUCCESS: Indicates successful execution.
        FILE_NOT_FOUND: Indicates that a file was not found.
        JSON_DECODE_ERROR: Indicates an error during JSON decoding.
        MISSING_ARGUMENTS: Indicates missing command-line arguments.
        RABBITMQ_CONNECTION_FAILED: Indicates failure to establish connection with RabbitMQ.
    """

    SUCCESS = 0
    FILE_NOT_FOUND = auto()
    JSON_DECODE_ERROR = auto()
    MISSING_ARGUMENTS = auto()
    RABBITMQ_CONNECTION_FAILED = auto()
