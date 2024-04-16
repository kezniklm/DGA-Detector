namespace Common.Models;

public record UserUpdateModel
{
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public required string PhoneNumber { get; set; }
    public required string? PhotoUrl { get; set; }
    public required bool SubscribedToNotifications { get; set; }
    public string? SecurityQuestion { get; set; }
    public string? SecurityAnswer { get; set; }
}
