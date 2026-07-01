using TodoApp.Domain.Common;

namespace TodoApp.Domain.Entities;

public class Category : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;
    public ICollection<TaskItem> Tasks { get; set; } = [];
}
