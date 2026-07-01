using TodoApp.Domain.Common;

namespace TodoApp.Domain.Entities;

public class TaskItem : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }

    public ICollection<Category> Categories { get; set; } = [];

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
