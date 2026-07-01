namespace TodoApp.Application.Common.Models;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
    public string RefreshCookieName { get; set; } = "refreshToken";
    public string RefreshCookiePath { get; set; } = "/api/auth";
    public bool RequireHttpsCookies { get; set; } = true;
    public string SameSiteMode { get; set; } = "None";
    public int MaxActiveSessionsPerUser { get; set; } = 5;
}
