using MediatR;
using TodoApp.Application.Common;
using TodoApp.Application.Interfaces;

namespace TodoApp.Application.Features.Categories;

public record DeleteCategoryCommand(Guid Id, Guid UserId) : IRequest;

public sealed class DeleteCategoryCommandHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ServiceException(ServiceErrorType.NotFound, $"Category {request.Id} not found.");

        if (category.UserId != request.UserId)
            throw new ServiceException(ServiceErrorType.Forbidden, "You do not own this category.");

        // EF is configured with OnDelete(SetNull) for TaskItem.CategoryId,
        // so associated tasks remain with CategoryId = null.
        await categoryRepository.DeleteAsync(category, cancellationToken);
    }
}
