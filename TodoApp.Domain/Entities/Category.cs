namespace TodoApp.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;
    public ICollection<TaskItem> Tasks { get; set; } = [];
}
