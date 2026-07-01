using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Interfaces;
using TodoApp.Domain.Entities;
using TodoApp.Infrastructure.Data;

namespace TodoApp.Infrastructure.Repositories;

public sealed class RefreshTokenRepository(AppDbContext context) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        await context.RefreshTokens.AddAsync(token, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        context.RefreshTokens.Update(token);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return context.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash, cancellationToken);
    }

    public Task<List<RefreshToken>> GetActiveTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return context.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAtUtc == null && r.ExpiresAtUtc > now)
            .OrderBy(r => r.CreatedAt) // Oldest first
            .ToListAsync(cancellationToken);
    }

    public Task<List<RefreshToken>> GetTokensByFamilyAsync(Guid familyId, CancellationToken cancellationToken = default)
    {
        return context.RefreshTokens
            .Where(r => r.FamilyId == familyId)
            .ToListAsync(cancellationToken);
    }
}
