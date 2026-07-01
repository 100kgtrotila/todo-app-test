using MediatR;
using TodoApp.Application.Common;
using TodoApp.Application.Interfaces;

namespace TodoApp.Application.Features.Tasks;

public record UpdateTaskCommand(
    Guid Id,
    string Title,
    string? Description,
    bool IsCompleted,
    DateTime? DueDate,
    Guid? CategoryId,
    Guid UserId) : IRequest<TaskDto>;

public sealed class UpdateTaskCommandHandler(ITaskRepository taskRepository)
    : IRequestHandler<UpdateTaskCommand, TaskDto>
{
    public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ServiceException(ServiceErrorType.NotFound, $"Task {request.Id} not found.");

        if (task.UserId != request.UserId)
            throw new ServiceException(ServiceErrorType.Forbidden, "You do not own this task.");

        task.Title = request.Title;
        task.Description = request.Description;
        task.IsCompleted = request.IsCompleted;
        task.DueDate = request.DueDate;
        task.CategoryId = request.CategoryId;
        task.UpdatedAt = DateTime.UtcNow;

        await taskRepository.UpdateAsync(task, cancellationToken);
        return new TaskDto(task.Id, task.Title, task.Description, task.IsCompleted,
            task.DueDate, task.CategoryId, task.Category?.Name, task.CreatedAt, task.UpdatedAt);
    }
}
