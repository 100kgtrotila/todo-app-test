using MediatR;
using TodoApp.Application.Common;
using TodoApp.Application.Interfaces;

namespace TodoApp.Application.Features.Tasks;

public record DeleteTaskCommand(Guid Id, Guid UserId) : IRequest;

public sealed class DeleteTaskCommandHandler(ITaskRepository taskRepository)
    : IRequestHandler<DeleteTaskCommand>
{
    public async Task Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ServiceException(ServiceErrorType.NotFound, $"Task {request.Id} not found.");

        if (task.UserId != request.UserId)
            throw new ServiceException(ServiceErrorType.Forbidden, "You do not own this task.");

        await taskRepository.DeleteAsync(task, cancellationToken);
    }
}
