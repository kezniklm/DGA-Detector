/**
 * @file BlacklistModel.cs
 *
 * @brief Defines the BlacklistModel record representing blacklisted domains.
 *
 * This file contains the definition of the BlacklistModel record, which represents blacklisted domains in the application. It implements the IModel interface from the BL.Models.Interfaces namespace.
 *
 * The main properties of this record include:
 * - DomainName: The domain name that is blacklisted.
 * - Added: The date and time when the domain was added to the blacklist.
 * - Id: The unique identifier of the blacklisted domain.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using BL.Models.Interfaces;

namespace BL.Models.Blacklist;

/// <summary>
///     Represents a record in the Blacklist, implementing the IModel interface.
/// </summary>
public record BlacklistModel : IModel
{
    /// <summary>
    ///     Gets or sets the domain name associated with the entry.
    /// </summary>
    public required string DomainName { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the entry was added to the blacklist.
    /// </summary>
    public required DateTime Added { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier of the blacklist entry.
    /// </summary>
    public required string Id { get; set; }
}
