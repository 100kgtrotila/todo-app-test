using MediatR;
using TodoApp.Application.Common;
using TodoApp.Application.Interfaces;

namespace TodoApp.Application.Features.Tasks;

public record UpdateTaskStatusCommand(Guid Id, bool IsCompleted, Guid UserId) : IRequest<TaskDto>;

public sealed class UpdateTaskStatusCommandHandler(ITaskRepository taskRepository)
    : IRequestHandler<UpdateTaskStatusCommand, TaskDto>
{
    public async Task<TaskDto> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ServiceException(ServiceErrorType.NotFound, $"Task {request.Id} not found.");

        if (task.UserId != request.UserId)
            throw new ServiceException(ServiceErrorType.Forbidden, "You do not own this task.");

        task.IsCompleted = request.IsCompleted;
        task.UpdatedAt = DateTime.UtcNow;

        await taskRepository.UpdateAsync(task, cancellationToken);
        return new TaskDto(task.Id, task.Title, task.Description, task.IsCompleted,
            task.DueDate, task.CategoryId, task.Category?.Name, task.CreatedAt, task.UpdatedAt);
    }
}
