/**
 * @file PublicController.cs
 *
 * @brief Defines API endpoints for public access.
 *
 * This file contains the implementation of the PublicController class, which defines API endpoints accessible to the public. It handles operations related to blacklists, whitelists, results, and user password management.
 *
 * The main functionalities of this controller include:
 * - Retrieving the total count of blacklist entries.
 * - Retrieving the total count of whitelist entries.
 * - Retrieving the total count of results.
 * - Filtering results by blacklist.
 * - Retrieving the number of domains today.
 * - Retrieving the number of positive results today.
 * - Resetting user passwords.
 * - Retrieving security questions for a given email.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using BL.Facades.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace APP.Controllers;

/// <summary>
///     Controller handling public endpoints for fetching counts and resetting passwords.
/// </summary>
[ApiController]
[Route("[controller]")]
public class PublicController(
    IBlacklistFacade blacklistFacade,
    IWhitelistFacade whitelistFacade,
    IResultFacade resultFacade,
    ILogger<PublicController> logger,
    UserManager<User> userManager)
    : ControllerBase
{
    /// <summary>
    ///     Endpoint to get the total count of blacklist entries.
    /// </summary>
    [HttpGet("blacklist/count")]
    public async Task<ActionResult<long>> GetTotalCountOfBlacklist()
    {
        try
        {
            logger.LogInformation("Fetching total count of blacklist entries.");
            long count = await blacklistFacade.GetNumberOfAllAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching total count of blacklist entries.");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Endpoint to get the total count of whitelist entries.
    /// </summary>
    [HttpGet("whitelist/count")]
    public async Task<ActionResult<long>> GetTotalCountOfWhiteList()
    {
        try
        {
            long count = await whitelistFacade.GetNumberOfAllAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting total count of whitelists.");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Endpoint to get the total count of results.
    /// </summary>
    [HttpGet("results/count")]
    public async Task<ActionResult<long>> GetTotalCount()
    {
        try
        {
            long count = await resultFacade.GetNumberOfAllAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting total count.");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Endpoint to get the count of results filtered by blacklist.
    /// </summary>
    [HttpGet("FilteredByBlacklist")]
    public async Task<ActionResult<long>> FilteredByBlacklist()
    {
        try
        {
            long count = await resultFacade.GetFilteredByBlacklistCountAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting filtered by blacklist count.");
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    /// <summary>
    ///     Endpoint to get the number of domains detected today.
    /// </summary>
    [HttpGet("NumberOfDomainsToday")]
    public async Task<ActionResult<long>> NumberOfDomainsToday()
    {
        try
        {
            long count = await resultFacade.GetNumberOfDomainsTodayAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting number of domains today.");
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    /// <summary>
    ///     Endpoint to get the number of positive detection results today.
    /// </summary>
    [HttpGet("PositiveResultsToday")]
    public async Task<ActionResult<long>> PositiveResultsToday()
    {
        try
        {
            long count = await resultFacade.GetPositiveDetectionResultsTodayAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting positive results today.");
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    /// <summary>
    ///     Endpoint to reset password for a user.
    /// </summary>
    /// <param name="model">Model containing user email, security question, and new password.</param>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] UserPasswordResetModel model)
    {
        User? user = await userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return NotFound("User not found");
        }

        if (!IsValidSecurityQuestionAnswer(user, model))
        {
            return BadRequest("Invalid security question answer.");
        }

        string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

        IdentityResult resetResult = await userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
        if (!resetResult.Succeeded)
        {
            return BadRequest(resetResult.Errors?.FirstOrDefault()?.Description ??
                              "Password reset failed with unknown error.");
        }

        return Ok("Password reset successfully");
    }

    /// <summary>
    ///     Validates the security question answer provided by the user.
    /// </summary>
    /// <param name="user">The user whose security question and answer are being validated.</param>
    /// <param name="model">Model containing security question and answer provided by the user.</param>
    /// <returns>True if the security question answer is valid, otherwise false.</returns>
    private bool IsValidSecurityQuestionAnswer(User user, UserPasswordResetModel model) =>
        user is { SecurityQuestion: not null, SecurityAnswer: not null } &&
        user.SecurityQuestion.Equals(model.SecurityQuestion, StringComparison.OrdinalIgnoreCase) &&
        user.SecurityAnswer.Equals(model.SecurityAnswer, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///     Endpoint to get the security question associated with a user by email.
    /// </summary>
    /// <param name="email">Email of the user.</param>
    [HttpGet("SecurityQuestion/{email}")]
    public async Task<IActionResult> GetSecurityQuestionByEmail(string email)
    {
        User? user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound("User not found");
        }

        if (string.IsNullOrEmpty(user.SecurityQuestion))
        {
            return NotFound("No security question set for this user");
        }

        return Ok(user.SecurityQuestion);
    }
}
