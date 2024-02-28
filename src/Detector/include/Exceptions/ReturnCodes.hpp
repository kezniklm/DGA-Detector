/**
 * @file ReturnCodes.hpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Defines the enum for return codes used in the network traffic monitoring and analysis application.
 *
 * This file declares the enum ReturnCodes, which contains various return codes used to indicate the success or failure of operations in the application.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 */

#pragma once

/**
 * @brief Enum for return codes used in the application.
 */
enum ReturnCodes
{
    /** Operation successful */
    SUCCESS,

    /** Operation failed */
    FAILURE,

    /** Help requested for command-line arguments */
    ARGUMENT_HELP,

    /** Failure in command-line argument validation */
    ARGUMENT_CHECK_FAILURE,

    /** Failure in creating a network analyser */
    NETWORK_ANALYSER_CREATION_FAILURE,

    /** Failure in creating a message publisher */
    MESSAGE_PUBLISHER_CREATION_FAILURE,

    /** Timeout occurred in message publishing */
    MESSAGE_PUBLISHER_TIMEOUT 
};
