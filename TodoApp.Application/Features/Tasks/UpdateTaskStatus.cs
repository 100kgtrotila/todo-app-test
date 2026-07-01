using ErrorOr;
using MediatR;
using TodoApp.Application.Common.Errors;
using TodoApp.Application.Interfaces;

namespace TodoApp.Application.Features.Tasks;

public record UpdateTaskStatusCommand(Guid Id, bool IsCompleted, Guid UserId) : IRequest<ErrorOr<TaskDto>>;

public sealed class UpdateTaskStatusCommandHandler(ITaskRepository taskRepository)
    : IRequestHandler<UpdateTaskStatusCommand, ErrorOr<TaskDto>>
{
    public async Task<ErrorOr<TaskDto>> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.Id, cancellationToken);
        if (task is null)
            return Errors.Tasks.NotFound(request.Id);

        if (task.UserId != request.UserId)
            return Errors.Tasks.ForbiddenAccess;

        task.IsCompleted = request.IsCompleted;
        task.UpdatedAt = DateTime.UtcNow;

        await taskRepository.UpdateAsync(task, cancellationToken);
        return new TaskDto(task.Id, task.Title, task.Description, task.IsCompleted,
            task.DueDate,
            task.Categories.Select(c => new TaskCategoryDto(c.Id, c.Name, c.Color)).ToList(),
            task.CreatedAt,
            task.UpdatedAt);
    }
}
