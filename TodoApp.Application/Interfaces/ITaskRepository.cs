using TodoApp.Application.Common;
using TodoApp.Application.Features.Tasks;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.Interfaces;

public interface ITaskRepository
{
    Task<(List<TaskItem> Items, int TotalCount)> GetPagedAsync(
        Guid userId,
        TaskQueryParams queryParams,
        CancellationToken ct = default);

    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TaskItem> CreateAsync(TaskItem task, CancellationToken ct = default);
    Task UpdateAsync(TaskItem task, CancellationToken ct = default);
    Task DeleteAsync(TaskItem task, CancellationToken ct = default);
}
