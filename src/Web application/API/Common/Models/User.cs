using Microsoft.AspNetCore.Identity;

namespace Common.Models;

public class User : IdentityUser
{
    public string? PhotoUrl { get; set; }

    public bool SubscribedToNotifications { get; set; }

    public string? DisplayUserName { get; set; }
}
