using MediatR;
using TodoApp.Application.Common;
using TodoApp.Application.Interfaces;

namespace TodoApp.Application.Features.Categories;

public record UpdateCategoryCommand(Guid Id, string Name, string? Color, Guid UserId) : IRequest<CategoryDto>;

public sealed class UpdateCategoryCommandHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ServiceException(ServiceErrorType.NotFound, $"Category {request.Id} not found.");

        if (category.UserId != request.UserId)
            throw new ServiceException(ServiceErrorType.Forbidden, "You do not own this category.");

        category.Name = request.Name;
        category.Color = request.Color;

        await categoryRepository.UpdateAsync(category, cancellationToken);
        return new CategoryDto(category.Id, category.Name, category.Color);
    }
}
