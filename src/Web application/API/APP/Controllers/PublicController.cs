using BL.Facades.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace APP.Controllers;

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

    private bool IsValidSecurityQuestionAnswer(User user, UserPasswordResetModel model) =>
        user is { SecurityQuestion: not null, SecurityAnswer: not null } &&
        user.SecurityQuestion.Equals(model.SecurityQuestion, StringComparison.OrdinalIgnoreCase) &&
        user.SecurityAnswer.Equals(model.SecurityAnswer, StringComparison.OrdinalIgnoreCase);

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
