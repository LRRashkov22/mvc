using Microsoft.AspNetCore.Mvc;
using WebApplication1;

namespace WarehouseApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserDto request)
    {
        var (user, error) = await authService.RegisterAsync(request);
        if (error is not null) return BadRequest(new { message = error });
        return Created("api/auth/login", new { userId = user!.Id, user.Username });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserDto request)
    {
        var (tokens, error) = await authService.LoginAsync(request);
        return error is not null ? Unauthorized(new { message = error }) : Ok(tokens);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequestDto request)
    {
        var tokens = await authService.RefreshTokensAsync(request);
        return tokens is null ? Unauthorized(new { message = "Invalid or expired refresh token." }) : Ok(tokens);
    }
}
