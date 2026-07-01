using MediatR;
using TodoApp.Application.Interfaces;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.Features.Tasks;

public record CreateTaskCommand(
    string Title,
    string? Description,
    DateTime? DueDate,
    Guid? CategoryId,
    Guid UserId) : IRequest<TaskDto>;

public sealed class CreateTaskCommandHandler(ITaskRepository taskRepository)
    : IRequestHandler<CreateTaskCommand, TaskDto>
{
    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false,
            DueDate = request.DueDate,
            CategoryId = request.CategoryId,
            UserId = request.UserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await taskRepository.CreateAsync(task, cancellationToken);
        return new TaskDto(task.Id, task.Title, task.Description, task.IsCompleted,
            task.DueDate, task.CategoryId, null, task.CreatedAt, task.UpdatedAt);
    }
}
