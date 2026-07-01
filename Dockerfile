# ============================================================
# Stage 1: Build + Publish
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for layer-cached restore
COPY TodoApp.sln .
COPY TodoApp.Domain/TodoApp.Domain.csproj           TodoApp.Domain/
COPY TodoApp.Application/TodoApp.Application.csproj TodoApp.Application/
COPY TodoApp.Infrastructure/TodoApp.Infrastructure.csproj TodoApp.Infrastructure/
COPY TodoApp.Api/TodoApp.Api.csproj                 TodoApp.Api/

RUN dotnet restore TodoApp.sln

# Copy all source and publish
COPY . .
RUN dotnet publish TodoApp.Api/TodoApp.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ============================================================
# Stage 2: Runtime image (minimal)
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

# Port that ASP.NET Core listens on inside the container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TodoApp.Api.dll"]
