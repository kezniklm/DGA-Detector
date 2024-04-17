/**
 * @file ResultModel.cs
 *
 * @brief Represents the result of a domain analysis.
 *
 * This file contains the definition of the ResultModel record, which represents the result of a domain analysis operation. It implements the IModel interface from the BL.Models.Interfaces namespace.
 *
 * The main properties of the ResultModel include:
 * - DomainName: The name of the domain being analyzed.
 * - Detected: The date and time when the analysis was performed.
 * - DidBlacklistHit: A boolean indicating whether the domain hit a blacklist.
 * - DangerousProbabilityValue: A numerical value representing the probability of danger associated with the domain.
 * - DangerousBoolValue: A boolean indicating whether the domain is considered dangerous.
 * - Id: The unique identifier of the result.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using BL.Models.Interfaces;

namespace BL.Models.Result;

/// <summary>
///     Represents the result of a domain analysis.
/// </summary>
public record ResultModel : IModel
{
    /// <summary>
    ///     Gets or sets the domain name.
    /// </summary>
    /// <value>The domain name.</value>
    public required string DomainName { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the domain was detected.
    /// </summary>
    /// <value>The date and time when the domain was detected.</value>
    public DateTime Detected { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the domain is detected in a blacklist.
    /// </summary>
    /// <value><c>true</c> if the domain is detected in a blacklist; otherwise, <c>false</c>.</value>
    public bool DidBlacklistHit { get; set; }

    /// <summary>
    ///     Gets or sets the probability value of domain being dangerous.
    /// </summary>
    /// <value>The probability value of domain being dangerous.</value>
    public double DangerousProbabilityValue { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the domain is considered dangerous.
    /// </summary>
    /// <value><c>true</c> if the domain is considered dangerous; otherwise, <c>false</c>.</value>
    public bool DangerousBoolValue { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier of the result.
    /// </summary>
    /// <value>The unique identifier of the result.</value>
    public required string Id { get; set; }
}
