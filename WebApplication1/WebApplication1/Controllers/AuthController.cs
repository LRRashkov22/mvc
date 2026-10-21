using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication1;
using WebApplication1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using WebApplication1.Services;
namespace Personal_Finance_Tracker.Controller;

[Route("api/[controller]")]
[ApiController]

public class AuthController(IAuthService AuthService) : ControllerBase {
    public static User user = new();

    [HttpPost("register")]
    public async Task<ActionResult<User>> Register(UserDto request) {
        var (user,error) = await AuthService.RegisterAsync(request);
        if (error != null) return BadRequest(error);

        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> login(UserDto request)
    {
        var (result, error) =  await AuthService.LoginAsync(request);
        if(error != null) return BadRequest(error);
        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public IActionResult AuthenticatedOnlyEndpoint() { 
        return Ok("You are authenticated and can access this endpoint.");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("Admin-only")]
    public IActionResult AdminOnlyEndpoint ()
    {
        return Ok("You are Admin.");
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto requestDto) { 
        var result = await AuthService.RefreshTokensAsync(requestDto);
        if(result is null) return Unauthorized("Invalid refresh token");
        return Ok(result);  
    }
}