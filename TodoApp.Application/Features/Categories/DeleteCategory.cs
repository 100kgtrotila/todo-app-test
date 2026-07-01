using ErrorOr;
using MediatR;
using TodoApp.Application.Common.Errors;
using TodoApp.Application.Interfaces;

namespace TodoApp.Application.Features.Categories;

public record DeleteCategoryCommand(Guid Id, Guid UserId) : IRequest<ErrorOr<Deleted>>;

public sealed class DeleteCategoryCommandHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<DeleteCategoryCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return Errors.Categories.NotFound(request.Id);

        if (category.UserId != request.UserId)
            return Errors.Categories.ForbiddenAccess;

        // EF automatically deletes rows in the task_categories join table
        await categoryRepository.DeleteAsync(category, cancellationToken);
        return Result.Deleted;
    }
}
