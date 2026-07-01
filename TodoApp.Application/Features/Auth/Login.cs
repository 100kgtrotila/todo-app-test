using MediatR;
using TodoApp.Application.Common;
using TodoApp.Application.Interfaces;

namespace TodoApp.Application.Features.Auth;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), cancellationToken);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new ServiceException(ServiceErrorType.Validation, "Invalid email or password.");

        var token = tokenService.GenerateToken(user);
        return new AuthResponse(token, user.Email, DateTime.UtcNow.AddHours(1));
    }
}
