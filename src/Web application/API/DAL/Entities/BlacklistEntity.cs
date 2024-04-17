/**
 * @file BlacklistEntity.cs
 *
 * @brief Represents an entity for blacklisted domains.
 *
 * This file contains the definition of the BlacklistEntity record, which represents an entity for blacklisted domains. It implements the IEntity interface.
 *
 * The main properties of this entity include:
 * - Added: DateTime representing the date when the domain was added to the blacklist.
 * - DomainName: String representing the name of the blacklisted domain.
 * - Id: String representing the unique identifier of the entity, annotated with BsonId and BsonRepresentation attributes for MongoDB.
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
///     Represents an entity for storing blacklisted domains.
/// </summary>
public record BlacklistEntity : IEntity
{
    /// <summary>
    ///     Gets or sets the date and time when the domain was added to the whitelist.
    /// </summary>
    /// <value>The date and time when the domain was added.</value>
    public required DateTime Added { get; set; }

    /// <summary>
    ///     Gets or sets the name of the whitelisted domain.
    /// </summary>
    /// <value>The name of the whitelisted domain.</value>
    public required string DomainName { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier for the whitelisted entity.
    /// </summary>
    /// <value>The unique identifier.</value>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }
}
