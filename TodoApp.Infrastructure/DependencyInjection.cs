using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Application.Interfaces;
using TodoApp.Domain.Entities;
using TodoApp.Infrastructure.Data;
using TodoApp.Infrastructure.Repositories;
using TodoApp.Infrastructure.Services;

namespace TodoApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // PostgreSQL
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly("TodoApp.Infrastructure"))
            .UseSnakeCaseNamingConvention());

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddSingleton<IPasswordHasher, PasswordHasherService>();

        // JWT
        services.Configure<TodoApp.Application.Common.Models.JwtSettings>(opts =>
            configuration.GetSection(TodoApp.Application.Common.Models.JwtSettings.SectionName).Bind(opts));

        var jwtSettings = new TodoApp.Application.Common.Models.JwtSettings();
        configuration.GetSection(TodoApp.Application.Common.Models.JwtSettings.SectionName).Bind(jwtSettings);

        if (jwtSettings.Key.Length < 32)
            throw new InvalidOperationException("JWT Key must be at least 32 characters long.");

        // Password hashing
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

        return services;
    }
}
