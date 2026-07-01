using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Api.Extensions;
using TodoApp.Application.Common;
using TodoApp.Application.Features.Tasks;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public sealed class TasksController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks(
        [FromQuery] TaskQueryParams queryParams,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await sender.Send(new GetTasksQuery(userId, queryParams), ct);
        return result.Match<IActionResult>(
            value => Ok(value),
            errors => errors.ToProblem(this));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTask(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await sender.Send(new GetTaskByIdQuery(id, userId), ct);
        return result.Match<IActionResult>(
            value => Ok(value),
            errors => errors.ToProblem(this));
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask(
        [FromBody] CreateTaskRequest request,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await sender.Send(
            new CreateTaskCommand(request.Title, request.Description, request.DueDate, request.CategoryIds, userId), ct);
        return result.Match<IActionResult>(
            value => CreatedAtAction(nameof(GetTask), new { id = value.Id }, value),
            errors => errors.ToProblem(this));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(
        Guid id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await sender.Send(
            new UpdateTaskCommand(id, request.Title, request.Description, request.IsCompleted,
                request.DueDate, request.CategoryIds, userId), ct);
        return result.Match<IActionResult>(
            value => Ok(value),
            errors => errors.ToProblem(this));
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTaskStatus(
        Guid id,
        [FromBody] UpdateTaskStatusRequest request,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await sender.Send(new UpdateTaskStatusCommand(id, request.IsCompleted, userId), ct);
        return result.Match<IActionResult>(
            value => Ok(value),
            errors => errors.ToProblem(this));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await sender.Send(new DeleteTaskCommand(id, userId), ct);
        return result.Match<IActionResult>(
            _ => NoContent(),
            errors => errors.ToProblem(this));
    }
}
