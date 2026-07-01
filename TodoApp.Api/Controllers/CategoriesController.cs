using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Api.Extensions;
using TodoApp.Application.Features.Categories;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public sealed class CategoriesController(ISender sender) : ControllerBase
{
    /// <summary>List all categories belonging to the authenticated user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await sender.Send(new GetCategoriesQuery(userId), ct);
        return result.Match<IActionResult>(
            value => Ok(value),
            errors => errors.ToProblem(this));
    }

    /// <summary>Create a new category for the authenticated user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await sender.Send(new CreateCategoryCommand(request.Name, request.Color, userId), ct);
        return result.Match<IActionResult>(
            value => StatusCode(StatusCodes.Status201Created, value),
            errors => errors.ToProblem(this));
    }

    /// <summary>Update an existing category. Only the owner can update.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await sender.Send(new UpdateCategoryCommand(id, request.Name, request.Color, userId), ct);
        return result.Match<IActionResult>(
            value => Ok(value),
            errors => errors.ToProblem(this));
    }

    /// <summary>
    /// Delete a category. Associated tasks are NOT deleted; their CategoryId becomes null.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await sender.Send(new DeleteCategoryCommand(id, userId), ct);
        return result.Match<IActionResult>(
            _ => NoContent(),
            errors => errors.ToProblem(this));
    }
}
