using TodoApp.Domain.Entities;

namespace TodoApp.Application.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Category>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<Category> CreateAsync(Category category, CancellationToken ct = default);
    Task UpdateAsync(Category category, CancellationToken ct = default);
    Task DeleteAsync(Category category, CancellationToken ct = default);
}
