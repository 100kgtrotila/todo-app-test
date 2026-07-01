using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Features.Tasks;
using TodoApp.Application.Interfaces;
using TodoApp.Domain.Entities;
using TodoApp.Infrastructure.Data;

namespace TodoApp.Infrastructure.Repositories;

public sealed class TaskRepository(AppDbContext context) : ITaskRepository
{
    public async Task<(List<TaskItem> Items, int TotalCount)> GetPagedAsync(
        Guid userId,
        TaskQueryParams queryParams,
        CancellationToken ct = default)
    {
        var query = context.Tasks
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var pattern = $"%{queryParams.Search.Trim()}%";
            query = query.Where(t =>
                EF.Functions.ILike(t.Title, pattern) ||
                (t.Description != null && EF.Functions.ILike(t.Description, pattern)));
        }

        if (queryParams.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == queryParams.CategoryId);

        if (queryParams.IsCompleted.HasValue)
            query = query.Where(t => t.IsCompleted == queryParams.IsCompleted);

        // Composable sort
        query = (queryParams.SortBy?.ToLower(), queryParams.SortDescending) switch
        {
            ("duedate", true)   => query.OrderByDescending(t => t.DueDate),
            ("duedate", false)  => query.OrderBy(t => t.DueDate),
            ("title", true)     => query.OrderByDescending(t => t.Title),
            ("title", false)    => query.OrderBy(t => t.Title),
            ("createdat", true) => query.OrderByDescending(t => t.CreatedAt),
            ("createdat", false) => query.OrderBy(t => t.CreatedAt),
            _                   => query.OrderByDescending(t => t.CreatedAt)
        };

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .AsNoTracking()
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Tasks
            .AsNoTracking()
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<TaskItem> CreateAsync(TaskItem task, CancellationToken ct = default)
    {
        context.Tasks.Add(task);
        await context.SaveChangesAsync(ct);
        return task;
    }

    public Task UpdateAsync(TaskItem task, CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);

    public async Task DeleteAsync(TaskItem task, CancellationToken ct = default)
    {
        context.Tasks.Remove(task);
        await context.SaveChangesAsync(ct);
    }
}
