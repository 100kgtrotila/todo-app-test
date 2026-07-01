using ErrorOr;
using MediatR;
using TodoApp.Application.Common.Errors;
using TodoApp.Application.Interfaces;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.Features.Tasks;

public record CreateTaskCommand(
    string Title,
    string? Description,
    DateTime? DueDate,
    List<Guid>? CategoryIds,
    Guid UserId) : IRequest<ErrorOr<TaskDto>>;

public sealed class CreateTaskCommandHandler(ITaskRepository taskRepository, ICategoryRepository categoryRepository)
    : IRequestHandler<CreateTaskCommand, ErrorOr<TaskDto>>
{
    public async Task<ErrorOr<TaskDto>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var categories = new List<Category>();
        if (request.CategoryIds?.Any() == true)
        {
            categories = await categoryRepository.GetByIdsAsync(request.CategoryIds, cancellationToken);
            if (categories.Count != request.CategoryIds.Count)
                return Error.Validation("Tasks.CategoryNotFound", "One or more categories not found.");
            
            if (categories.Any(c => c.UserId != request.UserId))
                return Errors.Tasks.ForbiddenAccess;
        }

        var now = DateTime.UtcNow;
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false,
            DueDate = request.DueDate,
            Categories = categories,
            UserId = request.UserId
        };

        await taskRepository.CreateAsync(task, cancellationToken);
        return new TaskDto(task.Id, task.Title, task.Description, task.IsCompleted,
            task.DueDate,
            task.Categories.Select(c => new TaskCategoryDto(c.Id, c.Name, c.Color)).ToList(),
            task.CreatedAt, task.UpdatedAt);
    }
}
