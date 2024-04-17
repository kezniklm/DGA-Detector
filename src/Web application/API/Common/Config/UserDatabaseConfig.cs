/**
 * @file UserDatabaseConfig.cs
 *
 * @brief Manages configuration settings for the user database.
 *
 * This file contains the implementation of the UserDatabaseConfig class, which is responsible for holding configuration settings for databases within the application. It supports conditional use of different database systems like MySQL, based on the provided configuration.
 *
 * The main functionalities of this file include:
 * - Storing whether to use MySQL or another database system.
 * - Holding the connection string necessary to connect to the database.
 * - Containing the name of the database to be used.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

namespace Common.Config;

/// <summary>
///     Represents the configuration settings for a user database.
///     This configuration includes details about whether to use MySQL, the connection string, and the database name.
/// </summary>
public class UserDatabaseConfig
{
    /// <summary>
    ///     Gets or sets a value indicating whether MySQL should be used as the database backend.
    /// </summary>
    /// <value>
    ///     <c>true</c> if MySQL is used; otherwise, <c>false</c>.
    /// </value>
    public required bool UseMySql { get; set; }

    /// <summary>
    ///     Gets or sets the database connection string.
    ///     This string contains all needed connection details such as the server location, credentials, and other parameters
    ///     required to establish a connection.
    /// </summary>
    /// <value>
    ///     The connection string for the database.
    /// </value>
    public required string ConnectionString { get; set; }

    /// <summary>
    ///     Gets or sets the name of the database.
    ///     This is the name of the database to which the application will connect.
    /// </summary>
    /// <value>
    ///     The name of the database.
    /// </value>
    public required string DatabaseName { get; set; }
}
