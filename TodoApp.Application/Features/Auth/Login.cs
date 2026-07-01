using ErrorOr;
using MediatR;
using TodoApp.Application.Common.Errors;
using TodoApp.Application.Interfaces;

namespace TodoApp.Application.Features.Auth;

public record LoginCommand(string Email, string Password, string? IpAddress = null) : IRequest<ErrorOr<AuthResult>>;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    Microsoft.Extensions.Options.IOptions<TodoApp.Application.Common.Models.JwtSettings> jwtOptions) : IRequestHandler<LoginCommand, ErrorOr<AuthResult>>
{
    private readonly TodoApp.Application.Common.Models.JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<ErrorOr<AuthResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), cancellationToken);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return Errors.Auth.InvalidCredentials;

        var token = tokenService.GenerateToken(user);
        
        // Refresh token generation
        var (rawToken, tokenHash) = refreshTokenService.GenerateTokenAndHash();
        var refreshToken = new TodoApp.Domain.Entities.RefreshToken
        {
            TokenHash = tokenHash,
            UserId = user.Id,
            FamilyId = Guid.NewGuid(), // New login creates a new session family
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
            CreatedByIp = request.IpAddress
        };

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        // Optional: Enforce max active sessions (fire and forget or inline)
        var activeTokens = await refreshTokenRepository.GetActiveTokensForUserAsync(user.Id, cancellationToken);
        if (activeTokens.Count > _jwtSettings.MaxActiveSessionsPerUser)
        {
            var tokensToRevoke = activeTokens.Take(activeTokens.Count - _jwtSettings.MaxActiveSessionsPerUser);
            foreach (var t in tokensToRevoke)
            {
                t.RevokedAtUtc = DateTime.UtcNow;
                t.RevokedByIp = request.IpAddress;
                await refreshTokenRepository.UpdateAsync(t, cancellationToken);
            }
        }

        var response = new AuthResponse(token, user.Email, DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes));
        return new AuthResult(response, rawToken);
    }
}
