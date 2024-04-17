/**
 * @file InvalidDeleteException.cs
 *
 * @brief Custom exception class for handling invalid delete operations.
 *
 * This file contains the implementation of the InvalidDeleteException class, which is a custom exception designed to be thrown during invalid delete operations within an application. It extends the standard Exception class provided by the .NET Framework, allowing for additional customization and handling specific to delete operations.
 *
 * The main functionalities of this file include:
 * - Defining a default constructor for the exception without any message.
 * - Allowing the creation of the exception with a specific error message.
 * - Enabling the inclusion of an inner exception to provide more detailed context about the error.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

namespace Common.Exceptions;

/// <summary>
///     Represents errors that occur during deletion operations when they are not allowed or invalid.
/// </summary>
public class InvalidDeleteException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidDeleteException" /> class.
    /// </summary>
    public InvalidDeleteException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidDeleteException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidDeleteException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidDeleteException" /> class with a specified error message and a
    ///     reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">
    ///     The exception that is the cause of the current exception, or a null reference if no inner exception
    ///     is specified.
    /// </param>
    public InvalidDeleteException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
