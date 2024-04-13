using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace APP.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UserController(UserManager<User> userManager) : ControllerBase
{
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

        IdentityResult result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok("User updated successfully");
    }

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
