using WebApplication1.Models;
namespace WebApplication1;

public interface IAuthService
{
    Task<(User? user, string? error)> RegisterAsync(UserDto request);
    Task<(TokenResponseDto? tokenRespone, string? error)> LoginAsync(UserDto request);
    Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
}