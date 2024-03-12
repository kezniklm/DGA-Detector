using System.Security.Claims;
using BL.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace APP.Controllers;

[ApiController]
[Route("")]
[Authorize]
public class AuthController(SignInManager<User> signInManager) : ControllerBase
{
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok(); // Returns a 200 OK response
    }

    [Authorize]
    [HttpGet("pingauth")]
    public IActionResult PingAuth()
    {
        var email = User.FindFirstValue(ClaimTypes.Email); // get the user's email from the claim
        return Ok(new { Email = email }); // return the email in JSON format
    }
}
