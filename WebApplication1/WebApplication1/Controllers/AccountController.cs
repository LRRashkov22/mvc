using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1;

namespace WebApplication1.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login() => View(new UserDto());

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(UserDto request)
    {
        var (tokens, error) = await _authService.LoginAsync(request);
        if (error is not null || tokens is null)
        {
            ModelState.Remove(nameof(request.Password));
            ModelState.AddModelError(string.Empty, error ?? "Unable to sign in.");
            return View(request);
        }

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokens.AccessToken);
        var role = jwt.Claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.Role || c.Type == "role")?.Value ?? "User";

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, request.Username),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register() => View(new UserDto());

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(UserDto request)
    {
        var (user, error) = await _authService.RegisterAsync(request);
        if (error is not null || user is null)
        {
            ModelState.Remove(nameof(request.Password));
            ModelState.AddModelError(string.Empty, error ?? "Unable to create account.");
            return View(request);
        }

        TempData["Success"] = "Account created. Sign in to continue.";
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
