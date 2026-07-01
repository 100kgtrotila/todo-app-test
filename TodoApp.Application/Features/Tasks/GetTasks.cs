using MediatR;
using TodoApp.Application.Common;
using TodoApp.Application.Interfaces;

namespace TodoApp.Application.Features.Tasks;

public record GetTasksQuery(Guid UserId, TaskQueryParams QueryParams) : IRequest<PagedResult<TaskDto>>;

public sealed class GetTasksQueryHandler(ITaskRepository taskRepository)
    : IRequestHandler<GetTasksQuery, PagedResult<TaskDto>>
{
    public async Task<PagedResult<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await taskRepository.GetPagedAsync(
            request.UserId,
            request.QueryParams,
            cancellationToken);

        var dtos = items.ConvertAll(t => new TaskDto(
            t.Id, t.Title, t.Description, t.IsCompleted,
            t.DueDate, t.CategoryId, t.Category?.Name,
            t.CreatedAt, t.UpdatedAt));

        return new PagedResult<TaskDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.QueryParams.Page,
            PageSize = request.QueryParams.PageSize
        };
    }
}
