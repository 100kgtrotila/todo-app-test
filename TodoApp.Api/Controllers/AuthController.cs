using MediatR;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Api.Extensions;
using TodoApp.Application.Features.Auth;

using Microsoft.Extensions.Options;
using TodoApp.Application.Common.Models; // For JwtSettings

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISender sender, IOptions<JwtSettings> jwtOptions) : ControllerBase
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new RegisterCommand(request.Email, request.Password), ct);
        return result.Match<IActionResult>(
            value => StatusCode(StatusCodes.Status201Created, value),
            errors => errors.ToProblem(this));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await sender.Send(new LoginCommand(request.Email, request.Password, ipAddress), ct);
        
        return result.Match<IActionResult>(
            value => 
            {
                SetTokenCookie(value.RefreshToken);
                return Ok(value.Response);
            },
            errors => errors.ToProblem(this));
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var refreshToken = Request.Cookies[_jwtSettings.RefreshCookieName];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "No refresh token provided." });

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await sender.Send(new RefreshCommand(refreshToken, ipAddress), ct);

        return result.Match<IActionResult>(
            value =>
            {
                SetTokenCookie(value.RefreshToken);
                return Ok(value.Response);
            },
            errors => Unauthorized(new { message = "Invalid or expired refresh token." }));
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var refreshToken = Request.Cookies[_jwtSettings.RefreshCookieName];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            await sender.Send(new LogoutCommand(refreshToken, ipAddress), ct);
        }

        RemoveTokenCookie();
        return Ok(new { message = "Logged out successfully." });
    }

    private void SetTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
            Path = _jwtSettings.RefreshCookiePath,
            Secure = _jwtSettings.RequireHttpsCookies,
            SameSite = Enum.TryParse<SameSiteMode>(_jwtSettings.SameSiteMode, true, out var sameSite) ? sameSite : SameSiteMode.Lax
        };
        
        Response.Cookies.Append(_jwtSettings.RefreshCookieName, token, cookieOptions);
    }

    private void RemoveTokenCookie()
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(-1),
            Path = _jwtSettings.RefreshCookiePath,
            Secure = _jwtSettings.RequireHttpsCookies,
            SameSite = Enum.TryParse<SameSiteMode>(_jwtSettings.SameSiteMode, true, out var sameSite) ? sameSite : SameSiteMode.Lax
        };
        
        Response.Cookies.Append(_jwtSettings.RefreshCookieName, "", cookieOptions);
    }
}
