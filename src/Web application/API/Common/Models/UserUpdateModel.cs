/**
 * @file UserUpdateModel.cs
 *
 * @brief Defines a record representing user update information.
 *
 * This file contains the definition of the UserUpdateModel record, which represents the data structure for updating user information. It includes properties for email, username, phone number, photo URL, notification subscription status, security question, and security answer.
 *
 * The main properties of this record include:
 * - Email: The email address of the user.
 * - UserName: The username of the user.
 * - PhoneNumber: The phone number of the user.
 * - PhotoUrl: The URL of the user's profile photo, nullable.
 * - SubscribedToNotifications: Indicates whether the user is subscribed to notifications.
 * - SecurityQuestion: The security question for the user's account, nullable.
 * - SecurityAnswer: The security answer for the user's account, nullable.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

namespace Common.Models;

/// <summary>
///     Represents the model for updating user information.
/// </summary>
public record UserUpdateModel
{
    /// <summary>
    ///     Gets or sets the email of the user. This field is required.
    /// </summary>
    /// <value>The email of the user.</value>
    public required string Email { get; set; }

    /// <summary>
    ///     Gets or sets the username of the user. This field is required.
    /// </summary>
    /// <value>The username of the user.</value>
    public required string UserName { get; set; }

    /// <summary>
    ///     Gets or sets the phone number of the user. This field is required.
    /// </summary>
    /// <value>The phone number of the user.</value>
    public required string PhoneNumber { get; set; }

    /// <summary>
    ///     Gets or sets the URL of the user's photo, if available.
    /// </summary>
    /// <value>The URL of the user's photo.</value>
    public required string? PhotoUrl { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the user is subscribed to notifications.
    /// </summary>
    /// <value>A value indicating whether the user is subscribed to notifications.</value>
    public required bool SubscribedToNotifications { get; set; }

    /// <summary>
    ///     Gets or sets the security question for the user, if set.
    /// </summary>
    /// <value>The security question for the user.</value>
    public string? SecurityQuestion { get; set; }

    /// <summary>
    ///     Gets or sets the answer to the security question, if set.
    /// </summary>
    /// <value>The answer to the security question.</value>
    public string? SecurityAnswer { get; set; }
}
