using Microsoft.AspNetCore.Identity;

namespace Common.Models;

public class User : IdentityUser
{
    public string? PhotoUrl { get; set; }
}
