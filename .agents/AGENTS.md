# AGENTS.md — Implementation Rules for This Repository

## Mission
Implement and maintain this backend as an interview-ready, production-style .NET Web API using clean architecture boundaries and modern best practices.

## Tech Baseline
- .NET 8
- ASP.NET Core Web API
- EF Core (Code First, migrations)
- Relational DB (SQL Server preferred)
- JWT Bearer Auth
- Docker + Docker Compose

---

## 1) Architecture Rules (Strict)

Use and keep these 4 layers:

1. **TodoApp.Api**
   - Controllers
   - Middleware
   - Authentication/authorization configuration
   - DI composition root
   - No business logic

2. **TodoApp.Application**
   - Service interfaces and implementations
   - DTOs / request-response contracts
   - Validation/business rules
   - Pagination models

3. **TodoApp.Domain**
   - Entities and core domain abstractions
   - No infrastructure dependencies

4. **TodoApp.Infrastructure**
   - DbContext
   - Entity configurations
   - Repository implementations
   - Persistence concerns only

### Layer dependency direction
- Api -> Application
- Application -> Domain (and abstractions)
- Infrastructure -> Application/Domain abstractions
- Domain -> (none)

Do not violate these boundaries.

---

## 2) Coding Standards

- Enable nullable reference types.
- Prefer explicit, clear naming over clever abstractions.
- Use async/await end-to-end for I/O and EF queries.
- Keep controllers thin; services contain business logic.
- Avoid static mutable state.
- Use `CancellationToken` in async service/repository methods where practical.
- Keep methods small and single-purpose.
- Return DTOs, not EF entities, from API boundaries.

---

## 3) API Design Rules

- Use RESTful route conventions: `/api/tasks`, `/api/categories`, etc.
- Use proper status codes:
  - 200 OK
  - 201 Created
  - 204 No Content
  - 400 Bad Request
  - 401 Unauthorized
  - 403 Forbidden (if applicable)
  - 404 Not Found
- Use consistent error format (ProblemDetails preferred).
- Validate all incoming DTOs.
- Never expose sensitive fields (e.g., PasswordHash).

---

## 4) Security Rules

- Passwords must be hashed (never plaintext).
- JWT key must come from configuration/environment.
- Protect non-auth endpoints with `[Authorize]`.
- Enforce ownership checks in service layer:
  - users can access only their own tasks/categories.
- Do not log secrets, tokens, or password values.
- CORS must be explicit and minimal for frontend origin(s).

---

## 5) Data & EF Core Rules

- Use Fluent API configurations for entity constraints/indexes.
- Add unique index on User.Email.
- Add FK constraints and sensible delete behavior.
- Use migrations for schema evolution.
- Avoid N+1 queries; use projection and include only needed fields.
- Use pagination with `Skip/Take`.
- Keep queries composable for filtering/search.

---

## 6) Docker Rules

- Use multi-stage API Dockerfile:
  - build/test/publish in SDK image
  - run in ASP.NET runtime image
- Provide `.dockerignore` to reduce context size.
- `docker-compose.yml` must run API + DB together.
- Configuration must be provided via environment variables in compose.
- Document exact run commands in README.

---

## 7) Observability & Reliability

- Add structured logging (default ASP.NET logging is fine; keep logs meaningful).
- Add `/health` endpoint.
- Use global exception middleware.
- Startup should fail fast on invalid configuration (especially JWT settings).

---

## 8) Testing Guidance (if implemented)

Minimum recommended:
- Unit tests for service logic (ownership, filtering/pagination logic)
- Optional integration tests for critical endpoints

If no tests are added, document why and propose next steps in README.

---

## 9) Definition of Good PR/Output

A good implementation must:
- Build cleanly
- Run locally and in Docker
- Apply migrations successfully
- Expose Swagger
- Pass core manual scenario:
  1. register
  2. login
  3. create category
  4. create task
  5. list tasks with search/filter/pagination
  6. update and delete task

---

## 10) Required Documentation

README must include:
- Architecture overview
- Project structure
- Setup (local + Docker)
- Migrations commands
- Environment variables table
- Auth flow explanation
- Pagination/filter/search behavior
- Assumptions and trade-offs

---

## 11) Anti-Patterns to Avoid

- Fat controllers
- Business logic in repositories
- Returning EF entities directly from controllers
- Hardcoded secrets
- Missing ownership checks
- Catch-all empty exception handling
- Large “god services” with unrelated responsibilities