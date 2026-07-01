namespace TodoApp.Application.Features.Auth;

public record AuthResult(
    AuthResponse Response,
    string RefreshToken
);
