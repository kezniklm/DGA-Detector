/**
 * @file UserPasswordChangeModel.cs
 *
 * @brief Represents a model for changing user passwords.
 *
 * This record defines the structure of data required to change a user's password. It includes fields for the current password and the new password.
 *
 * The main functionalities of this model include:
 * - Storing the current password.
 * - Storing the new password.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

namespace Common.Models;

/// <summary>
///     Represents a model for changing user password.
/// </summary>
public record UserPasswordChangeModel
{
    /// <summary>
    ///     Gets or sets the current password of the user.
    /// </summary>
    /// <value>This property holds the current password of the user which is used for verification purposes.</value>
    public required string CurrentPassword { get; set; }

    /// <summary>
    ///     Gets or sets the new password for the user.
    /// </summary>
    /// <value>
    ///     This property holds the new password that the user intends to set.
    /// </value>
    public required string NewPassword { get; set; }
}
