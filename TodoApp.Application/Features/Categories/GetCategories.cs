using MediatR;
using TodoApp.Application.Interfaces;

namespace TodoApp.Application.Features.Categories;

public record GetCategoriesQuery(Guid UserId) : IRequest<List<CategoryDto>>;

public sealed class GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        return categories.Select(c => new CategoryDto(c.Id, c.Name, c.Color)).ToList();
    }
}
