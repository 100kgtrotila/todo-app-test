using ErrorOr;
using MediatR;
using TodoApp.Application.Interfaces;

namespace TodoApp.Application.Features.Categories;

public record GetCategoriesQuery(Guid UserId) : IRequest<ErrorOr<List<CategoryDto>>>;

public sealed class GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<GetCategoriesQuery, ErrorOr<List<CategoryDto>>>
{
    public async Task<ErrorOr<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        return categories.ConvertAll(c => new CategoryDto(c.Id, c.Name, c.Color));
    }
}
