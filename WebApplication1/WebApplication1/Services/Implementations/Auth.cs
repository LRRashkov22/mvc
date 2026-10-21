using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApplication1.Data;
using System.Threading.Tasks;
using WebApplication1;
using  WebApplication1.Models;
namespace WebApplication1;


public class AuthService(WarehouseDbContext context, IConfiguration configuration) : IAuthService
{
    //Register-------------------------------------------------------------------
    public async Task<(User? user, string? error)> RegisterAsync(UserDto request)
    {
        //Username tests
        if (await context.User.AnyAsync(u => u.Username == request.Username)) return (null, "User already exists");
        if (request.Username.Length < 3) return (null, "Username must be at least 3 characters long");
        if (request.Username.Length > 20) return (null, "Username must be maximum 20 characters long");
        if (string.IsNullOrEmpty(request.Username)) return (null, "Username cannot be empty");
        if (request.Username.Any(ch => !char.IsLetterOrDigit(ch))) return (null, "Username cannot contain special symbols");
        if (request.Username.Any(ch => char.IsDigit(ch))) return (null, "Username cannot contain digits");
        //Password tests
        if (string.IsNullOrEmpty(request.Password)) return (null, "Password cannot be empty");
        if (request.Password.Length < 8) return (null, "Password must be at least 8 characters long");
        if (request.Password.Length > 30) return (null, "Password must be maximum 30 characters long");
        if (!request.Password.Any(char.IsUpper)) return (null, "Password must contain at least one uppercase letter");
        if (!request.Password.Any(char.IsLower)) return (null, "Password must contain at least one lowercase letter");
        if (!request.Password.Any(char.IsDigit)) return (null, "Password must contain at least one digit");
        if (!request.Password.Any(ch => !char.IsLetterOrDigit(ch))) return (null, "Password must contain at least one special character");

        //Create user
        var user = new User();
        var hashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);
        user.Username = request.Username;
        user.PasswordHash = hashedPassword;
        //Default role is User
        user.Role = "User";
        context.User.Add(user);
        await context.SaveChangesAsync();
        return (user, null);
    }
    //Register-------------------------------------------------------------------

    //Login----------------------------------------------------------------------
    public async Task<(TokenResponseDto? tokenRespone, string? error)> LoginAsync(UserDto request)
    {

        var user = await context.User.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user is null)
        {
            return (null, "User not found");
        }
        if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
            == PasswordVerificationResult.Failed)
        {
          
            await context.SaveChangesAsync();
            return (null, "User not found");
        }
        await context.SaveChangesAsync();

        return (await CreateTokenResponse(user), null);
    }
    //Login----------------------------------------------------------------------

    //Create Token---------------------------------------------------------------
    private string CreateToken(User user)
    {
        var claims = new List<Claim> {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Role, user.Role),
      };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        var tokenDescriptor = new JwtSecurityToken(
            issuer: configuration.GetValue<string>("AppSettings:Issuer"),
            audience: configuration.GetValue<string>("AppSettings:Audience"),
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
);
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
    //Create Token---------------------------------------------------------------

    private string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);

        var token = Convert.ToBase64String(randomBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        return token;
    }

    private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
    {
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await context.SaveChangesAsync();
        return refreshToken;
    }

    private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
    {
        var user = await context.User.FindAsync(userId);
        if (user is null || user.RefreshToken != refreshToken
            || user.RefreshTokenExpiryTime <= DateTime.UtcNow) return null;
        return user;
    }
    public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request)
    {
        var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
        if (user is null) return null;

        return await CreateTokenResponse(user);
    }



    //Token Response-------------------------------------------------------------
    //Used in both Login and Refresh TokenS methods 
    private async Task<TokenResponseDto> CreateTokenResponse(User user)
    {
        return new TokenResponseDto
        {
            AccessToken = CreateToken(user),
            RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
        };
    }
    //Token Response-------------------------------------------------------------

}