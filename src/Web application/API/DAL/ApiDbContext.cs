/**
 * @file ApiDbContext.cs
 *
 * @brief Provides database context for API using MongoDB.
 *
 * This file contains the implementation of the ApiDbContext class, which is responsible for providing a MongoDB context to the API. It utilizes the MongoDB.Driver library for managing database connections.
 *
 * The main functionalities of this file include:
 * - Creating database connections using a connection string.
 * - Providing a constructor for dependency injection with an IMongoClient instance.
 * - Offering an IMongoDatabase instance for data operations.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using MongoDB.Driver;

namespace DAL;

/// <summary>
///     Represents the database context for API operations using MongoDB.
///     This class provides the necessary connection and access to the MongoDB database specified by the connection string
///     and database name.
/// </summary>
public class ApiDbContext
{
    /// <summary>
    ///     Initializes a new instance of the ApiDbContext class using a connection string and a database name.
    /// </summary>
    /// <param name="connectionString">The MongoDB connection string to establish a connection to the database.</param>
    /// <param name="databaseName">The name of the database to access.</param>
    public ApiDbContext(string connectionString, string databaseName)
    {
        MongoClient client = new(connectionString);
        Database = client.GetDatabase(databaseName);
    }

    /// <summary>
    ///     Initializes a new instance of the ApiDbContext class using an existing MongoDB client and a database name.
    /// </summary>
    /// <param name="client">The MongoClient instance to use for database operations.</param>
    /// <param name="databaseName">The name of the database to access.</param>
    public ApiDbContext(IMongoClient client, string databaseName) => Database = client.GetDatabase(databaseName);

    /// <summary>
    ///     Gets the IMongoDatabase instance that provides access to the MongoDB operations.
    /// </summary>
    /// <value>
    ///     The IMongoDatabase instance associated with the specified database name.
    /// </value>
    public virtual IMongoDatabase Database { get; init; }
}
