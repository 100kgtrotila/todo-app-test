namespace TodoApp.Application.Interfaces;

public interface IRefreshTokenService
{
    /// <summary>
    /// Generates a cryptographically secure random token and its SHA-256 hash.
    /// </summary>
    (string Token, string Hash) GenerateTokenAndHash();

    /// <summary>
    /// Computes the SHA-256 hash for a given token.
    /// </summary>
    string ComputeHash(string token);
}
