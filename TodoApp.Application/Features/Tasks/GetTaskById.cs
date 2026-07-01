using ErrorOr;
using MediatR;
using TodoApp.Application.Common.Errors;
using TodoApp.Application.Interfaces;

namespace TodoApp.Application.Features.Tasks;

public record GetTaskByIdQuery(Guid Id, Guid UserId) : IRequest<ErrorOr<TaskDto>>;

public sealed class GetTaskByIdQueryHandler(ITaskRepository taskRepository)
    : IRequestHandler<GetTaskByIdQuery, ErrorOr<TaskDto>>
{
    public async Task<ErrorOr<TaskDto>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.Id, cancellationToken);
        if (task is null)
            return Errors.Tasks.NotFound(request.Id);

        if (task.UserId != request.UserId)
            return Errors.Tasks.ForbiddenAccess;

        return new TaskDto(task.Id, task.Title, task.Description, task.IsCompleted,
            task.DueDate,
            task.Categories.Select(c => new TaskCategoryDto(c.Id, c.Name, c.Color)).ToList(),
            task.CreatedAt,
            task.UpdatedAt);
    }
}
