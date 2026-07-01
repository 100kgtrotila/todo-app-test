using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Interfaces;
using TodoApp.Domain.Entities;
using TodoApp.Infrastructure.Data;

namespace TodoApp.Infrastructure.Repositories;

public sealed class CategoryRepository(AppDbContext context) : ICategoryRepository
{
    public async Task<List<Category>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await context.Categories.AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Categories
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<List<Category>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default) =>
        await context.Categories
            .Where(c => ids.Contains(c.Id))
            .ToListAsync(ct);

    public async Task<Category> CreateAsync(Category category, CancellationToken ct = default)
    {
        context.Categories.Add(category);
        await context.SaveChangesAsync(ct);
        return category;
    }

    public async Task UpdateAsync(Category category, CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);

    public async Task DeleteAsync(Category category, CancellationToken ct = default)
    {
        context.Categories.Remove(category);
        await context.SaveChangesAsync(ct);
    }
}
