using System.ComponentModel.DataAnnotations;

namespace TodoApp.Application.Features.Tasks;

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    bool IsCompleted,
    DateTime? DueDate,
    Guid? CategoryId,
    string? CategoryName,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateTaskRequest(
    [Required, MaxLength(200)] string Title,
    [MaxLength(2000)] string? Description,
    DateTime? DueDate,
    Guid? CategoryId
);

public record UpdateTaskRequest(
    [Required, MaxLength(200)] string Title,
    [MaxLength(2000)] string? Description,
    bool IsCompleted,
    DateTime? DueDate,
    Guid? CategoryId
);

public record UpdateTaskStatusRequest([Required] bool IsCompleted);
