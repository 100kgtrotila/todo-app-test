using TodoApp.Api.Extensions;
using TodoApp.Application.Features.Auth;
using TodoApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly));

builder.Services.AddApiPresentation(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<TodoApp.Infrastructure.Data.AppDbContext>("database");

var app = builder.Build();

await app.ApplyDatabaseMigrationsAsync();

app.ConfigurePipeline();
app.MapCustomHealthChecks();

app.Run();