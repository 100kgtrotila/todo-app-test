using System.Security.Claims;

namespace TodoApp.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("UserId claim (NameIdentifier) not found in token.");

        return Guid.Parse(value);
    }
}
