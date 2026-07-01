using System.ComponentModel.DataAnnotations;

namespace TodoApp.Application.Features.Tasks;

public record TaskCategoryDto(Guid Id, string Name, string? Color);

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    bool IsCompleted,
    DateTime? DueDate,
    List<TaskCategoryDto> Categories,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateTaskRequest(
    [Required, MaxLength(200)] string Title,
    [MaxLength(2000)] string? Description,
    DateTime? DueDate,
    List<Guid>? CategoryIds
);

public record UpdateTaskRequest(
    [Required, MaxLength(200)] string Title,
    [MaxLength(2000)] string? Description,
    bool IsCompleted,
    DateTime? DueDate,
    List<Guid>? CategoryIds
);

public record UpdateTaskStatusRequest([Required] bool IsCompleted);
