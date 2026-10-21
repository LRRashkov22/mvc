using System.ComponentModel.DataAnnotations;
namespace WebApplication1.Models;

public class User
{
    public int Id { get; set; }
    [Required]
    [Length(3, 20)]
    [RegularExpression(@"^[a-zA-Z]$",
        ErrorMessage = "Characters are not allowed.")]
    public string Username { get; set; } = string.Empty;
    [Required]
    [Length(8, 30)]
    public string PasswordHash { get; set; } = string.Empty;
    [Required]
    public string Role { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

}