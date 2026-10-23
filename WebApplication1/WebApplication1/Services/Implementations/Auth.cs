using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Models;
using WebApplication1.Repositories.Contracts;

namespace WebApplication1;

public sealed class AuthService(IUserRepository users, IConfiguration configuration) : IAuthService
{
    public async Task<(User? user, string? error)> RegisterAsync(UserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username)) return (null, "Username cannot be empty");
        if (request.Username.Length < 3) return (null, "Username must be at least 3 characters long");
        if (request.Username.Length > 20) return (null, "Username must be maximum 20 characters long");
        if (request.Username.Any(ch => !char.IsLetter(ch))) return (null, "Username can contain letters only");
        if (await users.UsernameExistsAsync(request.Username)) return (null, "User already exists");

        if (string.IsNullOrEmpty(request.Password)) return (null, "Password cannot be empty");
        if (request.Password.Length < 8) return (null, "Password must be at least 8 characters long");
        if (request.Password.Length > 30) return (null, "Password must be maximum 30 characters long");
        if (!request.Password.Any(char.IsUpper)) return (null, "Password must contain at least one uppercase letter");
        if (!request.Password.Any(char.IsLower)) return (null, "Password must contain at least one lowercase letter");
        if (!request.Password.Any(char.IsDigit)) return (null, "Password must contain at least one digit");
        if (!request.Password.Any(ch => !char.IsLetterOrDigit(ch))) return (null, "Password must contain at least one special character");

        var user = new User
        {
            Username = request.Username,
            Role = "User"
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);
        await users.AddAsync(user);
        await users.SaveChangesAsync();
        return (user, null);
    }

    public async Task<(TokenResponseDto? tokenRespone, string? error)> LoginAsync(UserDto request)
    {
        var user = await users.GetByUsernameAsync(request.Username);
        if (user is null) return (null, "Invalid username or password");

        var result = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed) return (null, "Invalid username or password");

        return (await CreateTokenResponse(user), null);
    }

    public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request)
    {
        var user = await users.GetByIdAsync(request.UserId);
        if (user is null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return null;
        return await CreateTokenResponse(user);
    }

    private string CreateToken(User user)
    {
        var tokenKey = configuration.GetValue<string>("AppSettings:Token");
        if (string.IsNullOrWhiteSpace(tokenKey))
            throw new InvalidOperationException("AppSettings:Token is not configured.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Role, user.Role)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        var token = new JwtSecurityToken(
            issuer: configuration.GetValue<string>("AppSettings:Issuer"),
            audience: configuration.GetValue<string>("AppSettings:Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private async Task<TokenResponseDto> CreateTokenResponse(User user)
    {
        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await users.SaveChangesAsync();
        return new TokenResponseDto
        {
            UserId = user.Id,
            AccessToken = CreateToken(user),
            RefreshToken = user.RefreshToken
        };
    }
}
