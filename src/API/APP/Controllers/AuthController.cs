using System.Security.Claims;
using Common.Models;
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
        return Ok();
    }

    [Authorize]
    [HttpGet("pingauth")]
    public IActionResult PingAuth()
    {
        string? email = User.FindFirstValue(ClaimTypes.Email);
        return Ok(new { Email = email });
    }
}
