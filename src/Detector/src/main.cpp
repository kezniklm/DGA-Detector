/**
 * @file main.cpp
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @brief Entry point for the network traffic monitoring and analysis application.
 *
 * This file contains the main function, which initializes the Detector object with command-line arguments and starts monitoring network traffic.
 *
 * @version 1.0
 * @date 2024-02-28
 * @copyright Copyright (c) 2024
 *
 */

#include "Detector.hpp"

/**
 * @brief Main function for the network traffic monitoring and analysis application.
 *
 * @param argc The number of command-line arguments.
 * @param argv An array of C-style strings containing the command-line arguments.
 * @return Returns EXIT_SUCCESS if the application runs successfully, or an error code if an exception occurs during initialization.
 */
int main(const int argc, const char **argv)
{
    std::unique_ptr<Detector> detector;
    try
    {
        detector = std::make_unique<Detector>(argc, argv);
    }
    catch (const DetectorException &e)
    {
        return e.GetCode();
    }
    catch (const std::exception)
    {
        return FAILURE;
    }

    detector->Run();

    return SUCCESS;
}