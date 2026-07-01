using System.Security.Cryptography;
using System.Text;
using TodoApp.Application.Interfaces;

namespace TodoApp.Infrastructure.Services;

public sealed class RefreshTokenService : IRefreshTokenService
{
    public (string Token, string Hash) GenerateTokenAndHash()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        
        var token = Convert.ToBase64String(randomBytes);
        var hash = ComputeHash(token);
        
        return (token, hash);
    }

    public string ComputeHash(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
