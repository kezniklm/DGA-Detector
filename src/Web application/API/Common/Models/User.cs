/**
 * @file User.cs
 *
 * @brief Represents a user in the application.
 *
 * This file contains the definition of the User class, which extends the IdentityUser class from Microsoft.AspNetCore.Identity. It represents a user in the application and includes additional properties such as PhotoUrl, SubscribedToNotifications, DisplayUserName, SecurityQuestion, and SecurityAnswer.
 *
 * The main functionalities of this file include:
 * - Defining properties for user data such as photo URL, display name, and security information.
 * - Inheriting from IdentityUser to leverage built-in user management features provided by Microsoft.AspNetCore.Identity.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using Microsoft.AspNetCore.Identity;

namespace Common.Models;

/// <summary>
///     Represents a user in the application.
/// </summary>
public class User : IdentityUser
{
    /// <summary>
    ///     Gets or sets the URL of the user's profile photo.
    /// </summary>
    /// <value>The URL of the user's profile photo.</value>
    public string? PhotoUrl { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the user is subscribed to notifications.
    /// </summary>
    /// <value>True if the user is subscribed to notifications; otherwise, false.</value>
    public bool SubscribedToNotifications { get; set; }

    /// <summary>
    ///     Gets or sets the display name of the user.
    /// </summary>
    /// <value>The display name of the user.</value>
    public string? DisplayUserName { get; set; }

    /// <summary>
    ///     Gets or sets the security question for account recovery.
    /// </summary>
    /// <value>The security question for account recovery.</value>
    public string? SecurityQuestion { get; set; }

    /// <summary>
    ///     Gets or sets the answer to the security question for account recovery.
    /// </summary>
    /// <value>The answer to the security question for account recovery.</value>
    public string? SecurityAnswer { get; set; }
}
