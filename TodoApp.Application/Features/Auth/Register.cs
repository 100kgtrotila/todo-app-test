using ErrorOr;
using MediatR;
using TodoApp.Application.Common.Errors;
using TodoApp.Application.Interfaces;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.Features.Auth;

public record RegisterCommand(string Email, string Password) : IRequest<ErrorOr<AuthResponse>>;

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IRequestHandler<RegisterCommand, ErrorOr<AuthResponse>>
{
    public async Task<ErrorOr<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var alreadyExists = await userRepository.ExistsByEmailAsync(request.Email, cancellationToken);
        if (alreadyExists)
            return Errors.Auth.DuplicateEmail(request.Email);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = passwordHasher.Hash(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.CreateAsync(user, cancellationToken);

        var token = tokenService.GenerateToken(user);
        return new AuthResponse(token, user.Email, DateTime.UtcNow.AddHours(1));
    }
}
