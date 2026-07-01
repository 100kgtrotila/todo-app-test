# TodoApp API

A production-style **.NET 8 REST API** for a To-Do application, built with clean 4-layer architecture, CQRS via MediatR, EF Core + PostgreSQL, JWT auth, and full Docker support.

---

## Architecture Overview

```
TodoApp.Api              → HTTP layer: Controllers, Swagger, Auth config, Exception handling
TodoApp.Application      → Business layer: CQRS Commands/Queries (MediatR), DTOs, Interfaces
TodoApp.Domain           → Core entities: User, Category, TaskItem (no dependencies)
TodoApp.Infrastructure   → Data layer: EF Core, Repositories, TokenService, PasswordHasher
```

### Layer Dependency Direction
```
Api → Application → Domain
Infrastructure → Application + Domain
```
Infrastructure is **never** referenced from Application — only interfaces are defined there and implemented in Infrastructure (Dependency Inversion Principle).

### Why CQRS?
Each feature is a vertical slice: `Commands` mutate state, `Queries` read it. This avoids "God Services" and makes each unit independently testable.

---

## Project Structure

```
TodoApp/
├── TodoApp.sln
├── Dockerfile
├── docker-compose.yml
├── .dockerignore
├── README.md
│
├── TodoApp.Domain/
│   └── Entities/
│       ├── User.cs
│       ├── Category.cs
│       └── TaskItem.cs
│
├── TodoApp.Application/
│   ├── Common/
│   │   ├── PagedResult.cs
│   │   └── ServiceException.cs
│   ├── Interfaces/
│   │   ├── IUserRepository.cs
│   │   ├── ICategoryRepository.cs
│   │   ├── ITaskRepository.cs
│   │   ├── ITokenService.cs
│   │   └── IPasswordHasher.cs
│   └── Features/
│       ├── Auth/         → Register.cs, Login.cs, AuthDtos.cs
│       ├── Categories/   → GetCategories, CreateCategory, UpdateCategory, DeleteCategory
│       └── Tasks/        → GetTasks, GetTaskById, CreateTask, UpdateTask, DeleteTask, UpdateTaskStatus
│
├── TodoApp.Infrastructure/
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── AppDbContextFactory.cs  (design-time)
│   │   ├── Configurations/         → Fluent API entity configs
│   │   └── Migrations/             → EF Core generated migrations
│   ├── Repositories/               → UserRepository, CategoryRepository, TaskRepository
│   ├── Services/                   → TokenService, PasswordHasherService, JwtSettings
│   └── DependencyInjection.cs
│
└── TodoApp.Api/
    ├── Controllers/                → AuthController, CategoriesController, TasksController
    ├── Exceptions/                 → GlobalExceptionHandler (IExceptionHandler)
    ├── Extensions/                 → ClaimsPrincipalExtensions
    ├── Program.cs
    ├── appsettings.json
    └── appsettings.Development.json
```

---

## Setup: Running Locally

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [PostgreSQL 14+](https://www.postgresql.org/) running locally
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

### 1. Configure the database connection

Edit `TodoApp.Api/appsettings.json` or set environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=TodoApp;Username=postgres;Password=postgres"
  },
  "JwtSettings": {
    "Key": "dev-only-secret-key-change-in-production-must-be-32chars",
    "Issuer": "TodoApp",
    "Audience": "TodoApp",
    "ExpirationMinutes": 60
  }
}
```

### 2. Apply migrations

```bash
dotnet ef database update \
  --project TodoApp.Infrastructure \
  --startup-project TodoApp.Api
```

> ⚠️ **Alternative:** Migrations are also applied automatically on startup (`MigrateAsync()`). This is intentional for dev/Docker convenience. See [Production Notes](#production-notes).

### 3. Run the API

```bash
dotnet run --project TodoApp.Api
```

Swagger UI is available at: **http://localhost:5000** (or the port shown in terminal)

---

## Setup: Running with Docker

### Prerequisites
- [Docker](https://www.docker.com/) + Docker Compose

### One-command startup

```bash
docker compose up --build
```

This will:
1. Build the API image (multi-stage: SDK → ASP.NET runtime)
2. Start PostgreSQL 16 (with health check)
3. Wait for DB to be healthy, then start the API
4. Apply EF migrations automatically on first boot

**Swagger UI:** http://localhost:8080

### Stop the application

```bash
docker compose down
```

### Stop and remove all data (reset volumes)

```bash
docker compose down -v
```

### View logs

```bash
docker compose logs -f api   # API logs only
docker compose logs -f db    # DB logs only
docker compose logs -f       # All logs
```

### Rebuild without cache

```bash
docker compose up --build --force-recreate
```

---

## Environment Variables

| Variable | Description | Default (dev) |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | `Host=localhost;...` |
| `JwtSettings__Key` | HS256 signing key (min 32 chars) | dev placeholder |
| `JwtSettings__Issuer` | JWT issuer claim | `TodoApp` |
| `JwtSettings__Audience` | JWT audience claim | `TodoApp` |
| `JwtSettings__ExpirationMinutes` | Token lifetime in minutes | `60` |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Development` |

> ⚠️ **Never commit real secrets.** Use Docker secrets, Azure Key Vault, or environment-level secrets management in production.

---

## Migrations Commands

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project TodoApp.Infrastructure \
  --startup-project TodoApp.Api

# Apply pending migrations
dotnet ef database update \
  --project TodoApp.Infrastructure \
  --startup-project TodoApp.Api

# Rollback to a specific migration
dotnet ef database update <PreviousMigrationName> \
  --project TodoApp.Infrastructure \
  --startup-project TodoApp.Api

# Remove last migration (if not yet applied to DB)
dotnet ef migrations remove \
  --project TodoApp.Infrastructure \
  --startup-project TodoApp.Api

# Generate SQL script for production
dotnet ef migrations script \
  --project TodoApp.Infrastructure \
  --startup-project TodoApp.Api \
  --output migration.sql
```

---

## Auth Flow

1. **Register** → `POST /api/auth/register` with `{email, password}` → receives JWT
2. **Login** → `POST /api/auth/login` with `{email, password}` → receives JWT access token in body, and `HttpOnly` cookie with `refreshToken`.
3. **Use JWT** → Add `Authorization: Bearer <token>` header to all protected requests
4. **Refresh Token** → `POST /api/auth/refresh` → reads `refreshToken` cookie, rotates it, and returns a new JWT access token.
5. **Logout** → `POST /api/auth/logout` → revokes the refresh token in the DB and clears the cookie.

### Secure Auth Setup
- **Short-Lived JWT:** Access tokens expire quickly (e.g. 15 minutes) to minimize risk.
- **Refresh Token Rotation:** Every refresh invalidates the old token and issues a new one.
- **Reuse Detection:** If a revoked/expired refresh token is used, the entire token family is revoked to prevent session hijacking.
- **HttpOnly Cookies:** Refresh tokens are transported via cookies, protecting against XSS attacks.

---

## Pagination, Search & Filtering

### `GET /api/tasks`

Supports the following query parameters:

| Parameter | Type | Default | Description |
|---|---|---|---|
| `page` | int | `1` | Page number (1-based) |
| `pageSize` | int | `10` | Items per page (max 100, auto-clamped) |
| `search` | string | — | Case-insensitive search in title + description |
| `categoryIds` | [guid] | — | Filter by categories (any match) |
| `isCompleted` | bool | — | Filter by completion status |
| `sortBy` | string | `createdat` | Sort field: `createdat`, `duedate`, `title` |
| `sortDescending` | bool | `true` | Sort direction |

### Response shape

```json
{
  "items": [...],
  "totalCount": 42,
  "page": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

---

## API Endpoints Summary

### Auth (`/api/auth`)
| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | ❌ | Register new user |
| POST | `/api/auth/login` | ❌ | Login, get JWT + set RT cookie |
| POST | `/api/auth/refresh` | ❌ | Rotate refresh token, get new JWT |
| POST | `/api/auth/logout` | ❌ | Revoke RT, clear cookie |

### Categories (`/api/categories`)
| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/categories` | ✅ | List own categories |
| POST | `/api/categories` | ✅ | Create category |
| PUT | `/api/categories/{id}` | ✅ | Update own category |
| DELETE | `/api/categories/{id}` | ✅ | Delete category (join table rows removed) |

### Tasks (`/api/tasks`)
| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/tasks` | ✅ | List tasks (paged, filtered, sorted) |
| GET | `/api/tasks/{id}` | ✅ | Get single task |
| POST | `/api/tasks` | ✅ | Create task |
| PUT | `/api/tasks/{id}` | ✅ | Full update task |
| PATCH | `/api/tasks/{id}/status` | ✅ | Toggle completion status only |
| DELETE | `/api/tasks/{id}` | ✅ | Delete task |

### Other
| Method | Path | Description |
|---|---|---|
| GET | `/health` | Health check (DB + API) |
| GET | `/swagger` | Swagger UI |

---

## Example curl Requests

### 1. Register

```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email": "alice@example.com", "password": "SecurePass123"}'
```

### 2. Login (save the token)

```bash
# Since the server sets an HttpOnly cookie for the refresh token, you may want to capture cookies using curl:
curl -c cookies.txt -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "alice@example.com", "password": "SecurePass123"}'
```

The response body will contain the `token` (Access Token) to be used for authorized requests.

### 2a. Refresh Token

```bash
# Uses the saved cookie to obtain a new access token
curl -b cookies.txt -c cookies.txt -X POST http://localhost:8080/api/auth/refresh
```

Or on Windows PowerShell:
```powershell
$response = Invoke-RestMethod -Uri "http://localhost:8080/api/auth/login" `
  -Method POST -ContentType "application/json" `
  -Body '{"email":"alice@example.com","password":"SecurePass123"}'
$TOKEN = $response.token
```

### 3. Create a category

```bash
curl -X POST http://localhost:8080/api/categories \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name": "Work", "color": "#3B82F6"}'
```

### 4. Create a task

```bash
curl -X POST http://localhost:8080/api/tasks \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Finish the API",
    "description": "Complete the backend for the interview",
    "dueDate": "2026-07-10T00:00:00Z",
    "categoryIds": ["<CATEGORY_ID_1_HERE>", "<CATEGORY_ID_2_HERE>"]
  }'
```

### 5. List tasks (with pagination + search + filter)

```bash
curl "http://localhost:8080/api/tasks?page=1&pageSize=5&search=api&isCompleted=false&sortBy=duedate&sortDescending=false" \
  -H "Authorization: Bearer $TOKEN"
```

### 6. Toggle task completion (PATCH)

```bash
curl -X PATCH "http://localhost:8080/api/tasks/<TASK_ID>/status" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"isCompleted": true}'
```

### 7. Full update (PUT)

```bash
curl -X PUT "http://localhost:8080/api/tasks/<TASK_ID>" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Finish the API (updated)",
    "description": "Almost done!",
    "isCompleted": false,
    "dueDate": "2026-07-15T00:00:00Z",
    "categoryIds": []
  }'
```

### 8. Delete task

```bash
curl -X DELETE "http://localhost:8080/api/tasks/<TASK_ID>" \
  -H "Authorization: Bearer $TOKEN"
```

### 9. Health check

```bash
curl http://localhost:8080/health
```

---

## Error Handling and Result Pattern

The application uses the **Result Pattern** with the `ErrorOr` library to handle expected business failures without using exceptions.

- **`ServiceException` is removed.** Business logic rules do not throw exceptions.
- Handlers return `ErrorOr<T>` containing either the result or a list of `Error` instances.
- Errors are centralized by feature in `TodoApp.Application/Common/Errors/` (e.g. `Errors.Tasks.NotFound(id)`).
- Controllers use `.Match(...)` to map the result or delegate errors to `ErrorOrExtensions.ToProblem(this)`, ensuring consistent, strongly-typed RFC 7807 Problem Details.

### Status Code Mapping

| ErrorOr Type | HTTP Status | Use Case |
|---|---|---|
| `Validation` | 400 Bad Request | Invalid credentials, DTO validation failures |
| `Forbidden` | 403 Forbidden | Ownership check failures |
| `NotFound` | 404 Not Found | Entity not found by ID |
| `Conflict` | 409 Conflict | Duplicate email on registration |
| `Unexpected` | 500 Internal Server Error | GlobalExceptionHandler fallback |

---

## Production Notes

### Migrations on startup
`MigrateAsync()` is called in `Program.cs` for dev/Docker convenience. In production:
- Run migrations as part of your CI/CD pipeline: `dotnet ef migrations script` → apply SQL
- Or use EF [Migration Bundles](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying#bundles)
- Wrap startup migration in an environment check if needed

### CORS
Currently configured for `http://localhost:4200` (Angular dev client). For production, set `AllowedOrigins` via environment variable.

### JWT Key
The minimum key length is 32 characters (enforced at startup). Use a cryptographically random 64-character key in production.

---

## Assumptions & Trade-offs

| Topic | Decision | Reason |
|---|---|---|
| DB | PostgreSQL (not SQL Server) | ARM64 compatible, lighter for Docker, allowed by PRD |
| CQRS | MediatR vertical slices | Avoids God Services, each handler independently testable |
| Services layer | Handlers are services | PRD's "4 layers" = Controllers / Services(Handlers) / Interfaces / Data |
| Auth | Refresh Tokens, Cookie transport | Production-ready rotation and family revocation implemented |
| DB Naming | Snake Case Convention | PostgreSQL standard (`task_categories`, `created_at`) |
| Cascade delete | Category → Task: Join Table | Deleting a category automatically drops its mapping in the join table without deleting tasks |
| No tests | Not included | Interview time constraint; unit tests for handler logic are the recommended next step |
| Swagger at `/` | Enabled in Development | Convenient for demo; restrict in production |
| Migration on startup | `MigrateAsync()` | One-command Docker startup convenience; see production notes |

---

## Testing (Recommended Next Steps)

No automated tests were added (time-boxed MVP). Recommended:
- **Unit tests** for each MediatR handler (mock `ITaskRepository`, `ICategoryRepository`)
- **Integration tests** for critical endpoints using `WebApplicationFactory<Program>` + Testcontainers for PostgreSQL
- **Test: ownership check** — verify user A cannot access user B's tasks/categories
