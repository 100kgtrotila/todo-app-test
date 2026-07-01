using System.ComponentModel.DataAnnotations;

namespace TodoApp.Application.Features.Categories;

public record CategoryDto(Guid Id, string Name, string? Color);

public record CreateCategoryRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(50)] string? Color
);

public record UpdateCategoryRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(50)] string? Color
);
