/**
 * @file UserPasswordResetModel.cs
 *
 * @brief Represents a model for user password reset information.
 *
 * This file contains the definition of the UserPasswordResetModel record, which represents the data structure for resetting a user's password. It includes properties for the user's email, new password, security question, and security answer.
 *
 * The main properties of this model include:
 * - Email: The email address of the user.
 * - NewPassword: The new password the user wants to set.
 * - SecurityQuestion: The security question chosen by the user.
 * - SecurityAnswer: The answer to the security question provided by the user.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

namespace Common.Models;

/// <summary>
///     Represents a model for resetting user passwords.
/// </summary>
public record UserPasswordResetModel
{
    /// <summary>
    ///     Gets or sets the email address of the user.
    /// </summary>
    /// <value>The email address of the user.</value>
    public required string Email { get; set; }

    /// <summary>
    ///     Gets or sets the new password for the user.
    /// </summary>
    /// <value>The new password for the user.</value>
    public required string NewPassword { get; set; }

    /// <summary>
    ///     Gets or sets the security question chosen by the user.
    /// </summary>
    /// <value>The security question chosen by the user.</value>
    public required string SecurityQuestion { get; set; }

    /// <summary>
    ///     Gets or sets the answer to the security question.
    /// </summary>
    /// <value>The answer to the security question.</value>
    public required string SecurityAnswer { get; set; }
}
