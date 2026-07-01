using TodoApp.Domain.Entities;

namespace TodoApp.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
