using ErrorOr;
using MediatR;
using TodoApp.Application.Common.Errors;
using TodoApp.Application.Interfaces;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.Features.Tasks;

public record UpdateTaskCommand(
    Guid Id,
    string Title,
    string? Description,
    bool IsCompleted,
    DateTime? DueDate,
    List<Guid>? CategoryIds,
    Guid UserId) : IRequest<ErrorOr<TaskDto>>;

public sealed class UpdateTaskCommandHandler(ITaskRepository taskRepository, ICategoryRepository categoryRepository)
    : IRequestHandler<UpdateTaskCommand, ErrorOr<TaskDto>>
{
    public async Task<ErrorOr<TaskDto>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.Id, cancellationToken);
        if (task is null)
            return Errors.Tasks.NotFound(request.Id);

        if (task.UserId != request.UserId)
            return Errors.Tasks.ForbiddenAccess;

        var categories = new List<Category>();
        if (request.CategoryIds?.Any() == true)
        {
            categories = await categoryRepository.GetByIdsAsync(request.CategoryIds, cancellationToken);
            if (categories.Count != request.CategoryIds.Count)
                return Error.Validation("Tasks.CategoryNotFound", "One or more categories not found.");
            
            if (categories.Any(c => c.UserId != request.UserId))
                return Errors.Tasks.ForbiddenAccess;
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.IsCompleted = request.IsCompleted;
        task.DueDate = request.DueDate;
        task.Categories = categories;

        await taskRepository.UpdateAsync(task, cancellationToken);
        return new TaskDto(task.Id, task.Title, task.Description, task.IsCompleted,
            task.DueDate,
            task.Categories.Select(c => new TaskCategoryDto(c.Id, c.Name, c.Color)).ToList(),
            task.CreatedAt, task.UpdatedAt);
    }
}
