/**
 * @file WhitelistModel.cs
 *
 * @brief Defines the WhitelistModel record representing a whitelisted domain.
 *
 * This file contains the definition of the WhitelistModel record, which represents a whitelisted domain. It implements the IModel interface from the BL.Models.Interfaces namespace.
 *
 * The main features of this file include:
 * - Defining properties for the domain name, date added, and ID of the whitelisted domain.
 * - Implementing the required interface members from IModel.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using BL.Models.Interfaces;

namespace BL.Models.Whitelist;

/// <summary>
///     Represents a model for whitelisted domains.
/// </summary>
public record WhitelistModel : IModel
{
    /// <summary>
    ///     Gets or sets the domain name.
    /// </summary>
    /// <value>The domain name.</value>
    public required string DomainName { get; set; }

    /// <summary>
    ///     Gets or sets the date when the domain was added to the whitelist.
    /// </summary>
    /// <value>The date when the domain was added.</value>
    public required DateTime Added { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier of the whitelisted domain.
    /// </summary>
    /// <value>The unique identifier of the whitelisted domain.</value>
    public required string Id { get; set; }
}
