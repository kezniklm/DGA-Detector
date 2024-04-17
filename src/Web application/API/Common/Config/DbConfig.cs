/**
 * @file DbConfig.cs
 *
 * @brief Provides configuration settings for the database connection.
 *
 * This class represents the configuration settings required for connecting to a database. It contains properties for the connection string, database name, and user database configuration.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

namespace Common.Config;

/// <summary>
///     Represents the configuration settings for a database.
/// </summary>
public class DbConfig
{
    /// <summary>
    ///     Gets or sets the connection string for the database.
    /// </summary>
    /// <value>The connection string.</value>
    public required string ConnectionString { get; set; }

    /// <summary>
    ///     Gets or sets the name of the database.
    /// </summary>
    /// <value>The name of the database.</value>
    public required string DatabaseName { get; set; }

    /// <summary>
    ///     Gets or sets the configuration settings for the user database.
    /// </summary>
    /// <value>The user database configuration.</value>
    public required UserDatabaseConfig UserDatabaseConfig { get; set; }
}
