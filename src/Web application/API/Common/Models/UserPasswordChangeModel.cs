namespace Common.Models;

public record UserPasswordChangeModel
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}
