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
    /// <summary>
    /// List tasks with optional pagination, search, category filter, completion filter, and sorting.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks(
        [FromQuery] TaskQueryParams queryParams,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await sender.Send(new GetTasksQuery(userId, queryParams), ct);
        return Ok(result);
    }

    /// <summary>Get a single task by ID. Only the owner can access it.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTask(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await sender.Send(new GetTaskByIdQuery(id, userId), ct);
        return Ok(result);
    }

    /// <summary>Create a new task for the authenticated user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask(
        [FromBody] CreateTaskRequest request,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await sender.Send(
            new CreateTaskCommand(request.Title, request.Description, request.DueDate, request.CategoryId, userId), ct);
        return CreatedAtAction(nameof(GetTask), new { id = result.Id }, result);
    }

    /// <summary>Fully update a task (all fields). Only the owner can update.</summary>
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
                request.DueDate, request.CategoryId, userId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Lightweight PATCH to toggle task completion status without sending the full task body.
    /// </summary>
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
        return Ok(result);
    }

    /// <summary>Delete a task. Only the owner can delete.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await sender.Send(new DeleteTaskCommand(id, userId), ct);
        return NoContent();
    }
}
