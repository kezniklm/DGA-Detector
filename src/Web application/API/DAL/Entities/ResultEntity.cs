/**
 * @file ResultEntity.cs
 *
 * @brief Represents a result entity in the data access layer.
 *
 * This file contains the definition of the ResultEntity record, which represents a result entity in the data access layer. It implements the IEntity interface and is used for storing results in the database.
 *
 * The main properties of the ResultEntity include:
 * - Detected: The date and time when the result was detected.
 * - DidBlacklistHit: A boolean indicating whether the result hit a blacklist.
 * - DangerousProbabilityValue: The probability value indicating the level of danger.
 * - DangerousBoolValue: A boolean indicating the danger level.
 * - DomainName: The name of the domain associated with the result.
 * - Id: The unique identifier of the result entity, represented as a string.
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
///     Represents an entity for detection results.
/// </summary>
public record ResultEntity : IEntity
{
    /// <summary>
    ///     Gets or sets the date and time when the detection was performed.
    /// </summary>
    /// <value>The date and time of detection.</value>
    public DateTime Detected { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the detection resulted in a hit on the blacklist.
    /// </summary>
    /// <value>True if the detection hit the blacklist; otherwise, false.</value>
    public bool DidBlacklistHit { get; set; }

    /// <summary>
    ///     Gets or sets the probability value indicating the level of danger.
    /// </summary>
    /// <value>The probability value of danger.</value>
    public double DangerousProbabilityValue { get; set; }

    /// <summary>
    ///     Gets or sets a boolean value indicating the dangerousness.
    /// </summary>
    /// <value>True if dangerous; otherwise, false.</value>
    public bool DangerousBoolValue { get; set; }

    /// <summary>
    ///     Gets or sets the domain name associated with the detection result.
    /// </summary>
    /// <value>The domain name.</value>
    public required string DomainName { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier of the result entity.
    /// </summary>
    /// <value>The unique identifier.</value>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }
}
