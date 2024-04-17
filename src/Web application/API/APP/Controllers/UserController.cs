/**
 * @file UserController.cs
 *
 * @brief Defines endpoints for managing user operations.
 *
 * This file contains the implementation of the UserController class, which defines endpoints for managing user operations such as retrieving, updating, deleting users, and changing passwords. The endpoints are secured with authentication using the Authorize attribute.
 *
 * The main functionalities of this file include:
 * - Retrieving user information.
 * - Updating user details.
 * - Deleting user accounts.
 * - Changing user passwords.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace APP.Controllers;

/// <summary>
///     Controller for user-related operations.
/// </summary>
[ApiController]
[Route("[controller]")]
[Authorize]
public class UserController(UserManager<User> userManager) : ControllerBase
{
    /// <summary>
    ///     Get details of the currently authenticated user.
    /// </summary>
    /// <returns>The details of the currently authenticated user.</returns>
    [HttpGet("get")]
    public async Task<ActionResult<User>> GetUser()
    {
        User? user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return NotFound("User not found");
        }

        if (user.DisplayUserName is null)
        {
            user.DisplayUserName = user.UserName;
            await userManager.UpdateAsync(user);
        }

        return Ok(user);
    }

    /// <summary>
    ///     Update details of the currently authenticated user.
    /// </summary>
    /// <param name="model">The model containing updated user details.</param>
    /// <returns>ActionResult indicating success or failure of the update operation.</returns>
    [HttpPut("update")]
    public async Task<IActionResult> UpdateUser(UserUpdateModel model)
    {
        User? user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound("User not found");
        }

        user.Email = model.Email;
        user.NormalizedEmail = model.Email.ToUpper();
        user.UserName = model.Email;
        user.NormalizedUserName = model.Email.ToUpper();
        user.DisplayUserName = model.UserName;
        user.PhoneNumber = model.PhoneNumber;
        user.PhotoUrl = model.PhotoUrl;
        user.SubscribedToNotifications = model.SubscribedToNotifications;

        if (model.SecurityAnswer is not null && model.SecurityQuestion is not null)
        {
            user.SecurityQuestion = model.SecurityQuestion;
            user.SecurityAnswer = model.SecurityAnswer;
        }

        IdentityResult result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok("User updated successfully");
    }

    /// <summary>
    ///     Delete a user by user ID.
    /// </summary>
    /// <param name="userId">The ID of the user to be deleted.</param>
    /// <returns>ActionResult indicating success or failure of the delete operation.</returns>
    [HttpPost("delete/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        User? user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound("User not found");
        }

        IdentityResult result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok("User deleted successfully");
    }

    /// <summary>
    ///     Change the password of the currently authenticated user.
    /// </summary>
    /// <param name="model">The model containing the current and new passwords.</param>
    /// <returns>ActionResult indicating success or failure of the password change operation.</returns>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] UserPasswordChangeModel model)
    {
        User? user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound("User not found");
        }

        IdentityResult result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok("Password changed successfully");
    }
}
