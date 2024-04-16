namespace Common.Models;

public record UserPasswordResetModel
{
    public required string Email { get; set; }
    public required string NewPassword { get; set; }
    public required string SecurityQuestion { get; set; }
    public required string SecurityAnswer { get; set; }
}
