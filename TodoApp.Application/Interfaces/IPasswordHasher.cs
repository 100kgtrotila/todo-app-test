namespace TodoApp.Application.Interfaces;

/// <summary>
/// Password hashing abstraction. Defined in Application so handlers stay free
/// of Microsoft.Extensions.Identity.Core dependency.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
