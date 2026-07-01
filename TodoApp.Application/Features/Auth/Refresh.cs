using ErrorOr;
using MediatR;
using Microsoft.Extensions.Options;
using TodoApp.Application.Common.Errors;
using TodoApp.Application.Interfaces;
using TodoApp.Domain.Entities;
using TodoApp.Application.Common.Models; // Ensure using JwtSettings

namespace TodoApp.Application.Features.Auth;

public record RefreshCommand(string RefreshToken, string? IpAddress = null) : IRequest<ErrorOr<AuthResult>>;

public sealed class RefreshCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IRefreshTokenService refreshTokenService,
    IUserRepository userRepository,
    ITokenService tokenService,
    IOptions<JwtSettings> jwtOptions) : IRequestHandler<RefreshCommand, ErrorOr<AuthResult>>
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<ErrorOr<AuthResult>> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = refreshTokenService.ComputeHash(request.RefreshToken);
        var existingToken = await refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken);

        if (existingToken is null)
            return Errors.Auth.InvalidCredentials; // Treat invalid RT same as invalid credentials

        // Detect reuse: If token is revoked or expired
        if (!existingToken.IsActive)
        {
            // Revoke the entire family
            var familyTokens = await refreshTokenRepository.GetTokensByFamilyAsync(existingToken.FamilyId, cancellationToken);
            foreach (var t in familyTokens.Where(x => x.IsActive))
            {
                t.RevokedAtUtc = DateTime.UtcNow;
                t.RevokedByIp = request.IpAddress;
                await refreshTokenRepository.UpdateAsync(t, cancellationToken);
            }
            return Errors.Auth.InvalidCredentials;
        }

        var user = await userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);
        if (user is null)
            return Errors.Auth.InvalidCredentials;

        // Rotate token
        var (newRawToken, newHash) = refreshTokenService.GenerateTokenAndHash();
        
        var newRefreshToken = new RefreshToken
        {
            TokenHash = newHash,
            UserId = user.Id,
            FamilyId = existingToken.FamilyId, // Keep same family
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
            CreatedByIp = request.IpAddress
        };

        // Revoke the old token
        existingToken.RevokedAtUtc = DateTime.UtcNow;
        existingToken.RevokedByIp = request.IpAddress;
        existingToken.ReplacedByTokenId = newRefreshToken.Id; // Note: we'd need to generate ID first, or just save it.
        
        // EF Core will generate the ID for newRefreshToken on Add, but we can set a Guid now
        newRefreshToken.Id = Guid.NewGuid();
        existingToken.ReplacedByTokenId = newRefreshToken.Id;

        await refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);
        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        // Generate new JWT Access Token
        var newJwt = tokenService.GenerateToken(user);
        var response = new AuthResponse(newJwt, user.Email, DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes));

        return new AuthResult(response, newRawToken);
    }
}
