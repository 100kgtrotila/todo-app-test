using ErrorOr;
using MediatR;
using TodoApp.Application.Interfaces;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.Features.Categories;

public record CreateCategoryCommand(string Name, string? Color, Guid UserId) : IRequest<ErrorOr<CategoryDto>>;

public sealed class CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<CreateCategoryCommand, ErrorOr<CategoryDto>>
{
    public async Task<ErrorOr<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Color = request.Color,
            UserId = request.UserId
        };

        await categoryRepository.CreateAsync(category, cancellationToken);
        return new CategoryDto(category.Id, category.Name, category.Color);
    }
}
