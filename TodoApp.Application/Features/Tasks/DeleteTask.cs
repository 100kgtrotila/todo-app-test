using ErrorOr;
using MediatR;
using TodoApp.Application.Common.Errors;
using TodoApp.Application.Interfaces;

namespace TodoApp.Application.Features.Tasks;

public record DeleteTaskCommand(Guid Id, Guid UserId) : IRequest<ErrorOr<Deleted>>;

public sealed class DeleteTaskCommandHandler(ITaskRepository taskRepository)
    : IRequestHandler<DeleteTaskCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.Id, cancellationToken);
        if (task is null)
            return Errors.Tasks.NotFound(request.Id);

        if (task.UserId != request.UserId)
            return Errors.Tasks.ForbiddenAccess;

        await taskRepository.DeleteAsync(task, cancellationToken);
        return Result.Deleted;
    }
}
