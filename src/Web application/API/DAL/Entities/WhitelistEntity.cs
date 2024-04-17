/**
 * @file WhitelistEntity.cs
 *
 * @brief Represents a whitelist entity in the data access layer.
 *
 * This file contains the definition of the WhitelistEntity record, which represents an entity in the data access layer for whitelisting domains. It implements the IEntity interface.
 *
 * The main functionalities of this file include:
 * - Defining properties for the whitelist entity, such as Added, DomainName, and Id.
 * - Implementing the IEntity interface for compatibility with the data access layer.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using DAL.Entities.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL.Entities;

/// <summary>
///     Represents an entity for whitelisted domains.
/// </summary>
public record WhitelistEntity : IEntity
{
    /// <summary>
    ///     Gets or sets the date and time when the domain was added to the whitelist.
    /// </summary>
    /// <value>The date and time when the domain was added.</value>
    public required DateTime Added { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier for the entity.
    /// </summary>
    /// <value>The unique identifier.</value>
    public required string DomainName { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier for the entity.
    /// </summary>
    /// <value>The unique identifier.</value>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }
}
