using MediatR;
using TodoApp.Application.Interfaces;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.Features.Categories;

public record CreateCategoryCommand(string Name, string? Color, Guid UserId) : IRequest<CategoryDto>;

public sealed class CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
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
