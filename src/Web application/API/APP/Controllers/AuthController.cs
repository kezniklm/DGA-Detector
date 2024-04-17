/**
 * @file AuthController.cs
 *
 * @brief Defines the AuthController class responsible for authentication-related endpoints.
 *
 * This file contains the implementation of the AuthController class, which handles authentication-related HTTP endpoints for the application. These endpoints include user logout and authentication ping functionalities.
 *
 * The main functionalities of this class include:
 * - Logging out the current user via the HTTP POST method at the "logout" endpoint.
 * - Responding to authentication ping requests via the HTTP GET method at the "pingauth" endpoint.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using System.Security.Claims;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace APP.Controllers;

/// <summary>
///     Controller responsible for authentication-related endpoints.
/// </summary>
[ApiController]
[Route("")]
[Authorize]
public class AuthController(SignInManager<User> signInManager) : ControllerBase
{
    /// <summary>
    ///     Endpoint for user logout.
    /// </summary>
    /// <returns>An IActionResult indicating the result of the operation.</returns>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok();
    }

    /// <summary>
    ///     Endpoint to ping authentication, returns the email of the authenticated user.
    /// </summary>
    /// <returns>An IActionResult containing the authenticated user's email.</returns>
    [Authorize]
    [HttpGet("pingauth")]
    public IActionResult PingAuth()
    {
        string? email = User.FindFirstValue(ClaimTypes.Email);
        return Ok(new { Email = email });
    }
}
