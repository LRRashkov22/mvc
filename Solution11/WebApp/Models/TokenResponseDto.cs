namespace WebApplication1;

public class TokenResponseDto
{
    public int UserId { get; set; }
    public required string  AccessToken { get; set; }
    public required string  RefreshToken { get; set; }
}