using ErrorOr;
using MediatR;
using TodoApp.Application.Common.Errors;
using TodoApp.Application.Interfaces;

namespace TodoApp.Application.Features.Auth;

public record LogoutCommand(string RefreshToken, string? IpAddress = null) : IRequest<ErrorOr<Success>>;

public sealed class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IRefreshTokenService refreshTokenService) : IRequestHandler<LogoutCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = refreshTokenService.ComputeHash(request.RefreshToken);
        var existingToken = await refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken);

        if (existingToken is not null && existingToken.IsActive)
        {
            existingToken.RevokedAtUtc = DateTime.UtcNow;
            existingToken.RevokedByIp = request.IpAddress;
            await refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);
        }

        return Result.Success;
    }
}
