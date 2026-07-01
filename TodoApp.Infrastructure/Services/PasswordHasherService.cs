using Microsoft.AspNetCore.Identity;
using TodoApp.Application.Interfaces;
using TodoApp.Domain.Entities;

namespace TodoApp.Infrastructure.Services;

/// <summary>
/// Wraps ASP.NET Core's <see cref="PasswordHasher{TUser}"/> behind the Application-layer abstraction.
/// </summary>
public sealed class PasswordHasherService(IPasswordHasher<User> hasher) : IPasswordHasher
{
    public string Hash(string password) =>
        hasher.HashPassword(null!, password);

    public bool Verify(string password, string hash) =>
        hasher.VerifyHashedPassword(null!, hash, password) != PasswordVerificationResult.Failed;
}
