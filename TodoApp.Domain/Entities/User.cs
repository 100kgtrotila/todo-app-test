using TodoApp.Domain.Common;

namespace TodoApp.Domain.Entities;

public class User : AuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<Category> Categories { get; set; } = [];
    public ICollection<TaskItem> Tasks { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
