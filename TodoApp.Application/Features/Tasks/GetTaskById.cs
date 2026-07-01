using MediatR;
using TodoApp.Application.Common;
using TodoApp.Application.Interfaces;

namespace TodoApp.Application.Features.Tasks;

public record GetTaskByIdQuery(Guid Id, Guid UserId) : IRequest<TaskDto>;

public sealed class GetTaskByIdQueryHandler(ITaskRepository taskRepository)
    : IRequestHandler<GetTaskByIdQuery, TaskDto>
{
    public async Task<TaskDto> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ServiceException(ServiceErrorType.NotFound, $"Task {request.Id} not found.");

        if (task.UserId != request.UserId)
            throw new ServiceException(ServiceErrorType.Forbidden, "You do not own this task.");

        return new TaskDto(task.Id, task.Title, task.Description, task.IsCompleted,
            task.DueDate, task.CategoryId, task.Category?.Name, task.CreatedAt, task.UpdatedAt);
    }
}
