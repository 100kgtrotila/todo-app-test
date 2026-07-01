using ErrorOr;
using MediatR;
using TodoApp.Application.Common.Errors;
using TodoApp.Application.Interfaces;

namespace TodoApp.Application.Features.Categories;

public record UpdateCategoryCommand(Guid Id, string Name, string? Color, Guid UserId) : IRequest<ErrorOr<CategoryDto>>;

public sealed class UpdateCategoryCommandHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<UpdateCategoryCommand, ErrorOr<CategoryDto>>
{
    public async Task<ErrorOr<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return Errors.Categories.NotFound(request.Id);

        if (category.UserId != request.UserId)
            return Errors.Categories.ForbiddenAccess;

        category.Name = request.Name;
        category.Color = request.Color;

        await categoryRepository.UpdateAsync(category, cancellationToken);
        return new CategoryDto(category.Id, category.Name, category.Color);
    }
}
