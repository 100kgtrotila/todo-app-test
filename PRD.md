# Backend PRD — To-Do Application API

## 1. Objective
Build a REST API for a To-Do app with authentication, task/category management, pagination, search, and filtering, using .NET + EF Core + relational DB with strict 4-layer architecture and Docker support.

## 2. Scope (Backend MVP)
Included:
- Auth: register/login/logout (JWT-based)
- Tasks: create/read/update/delete
- Categories: create/read/update/delete
- Pagination on tasks list
- Search + filter by category (and optional completion status)
- Per-user data isolation
- Dockerized local/dev run

Excluded:
- Notifications/reminders
- File attachments
- Admin roles/permissions
- Real-time features

## 3. Architecture Requirements (MANDATORY)
Use 4 layers:
1. Controllers (HTTP endpoints only, validation + response handling)
2. Services (business logic, ownership checks)
3. Interfaces (contracts for services/repositories)
4. Data Access (EF Core DbContext, repositories, migrations)

## 4. Technology Constraints
- ASP.NET Core Web API (.NET 8 preferred)
- EF Core (Code First)
- Relational DB:
  - Primary target: MS SQL Server
  - For Docker/local simplicity: SQL Server container (preferred) or PostgreSQL if explicitly documented
- Built-in Dependency Injection
- JWT authentication
- Swagger/OpenAPI enabled
- Docker + Docker Compose required

## 5. Domain Model

### User
- Id
- Email (unique, required)
- PasswordHash (required)
- CreatedAt

### Category
- Id
- Name (required, max length)
- Color (optional)
- UserId (FK, required)

### TaskItem
- Id
- Title (required, max length)
- Description (optional)
- IsCompleted (bool)
- DueDate (optional)
- CategoryId (optional FK)
- UserId (FK, required)
- CreatedAt
- UpdatedAt

## 6. Functional Requirements

### FR-1 Auth
- Register user with email/password
- Login returns JWT token
- Protected endpoints require valid JWT
- Logout endpoint exists (stateless: informational response; token removal on client)

### FR-2 Categories
- Create category (owned by current user)
- List current user categories
- Update own category
- Delete own category
- Must not access/modify other users’ categories

### FR-3 Tasks CRUD
- Create task (owned by current user)
- Get task by id (only own)
- Update own task
- Delete own task
- Must not access/modify other users’ tasks

### FR-4 Task List Query
`GET /api/tasks` supports:
- `page` (default 1)
- `pageSize` (default 10, max 100)
- `search` (title/description contains)
- `categoryId` (optional)
- `isCompleted` (optional)

Response:
- `items`
- `totalCount`
- `page`
- `pageSize`

### FR-5 Validation & Errors
- Validate DTOs (required fields, lengths, formats)
- Consistent error format for 400/401/403/404/500
- Global exception handling middleware
- Use RFC7807 ProblemDetails where reasonable

## 7. API Endpoints

### Auth
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/logout`

### Categories
- `GET /api/categories`
- `POST /api/categories`
- `PUT /api/categories/{id}`
- `DELETE /api/categories/{id}`

### Tasks
- `GET /api/tasks`
- `GET /api/tasks/{id}`
- `POST /api/tasks`
- `PUT /api/tasks/{id}`
- `DELETE /api/tasks/{id}`

## 8. Non-Functional Requirements
- Strict separation of concerns
- Readable, interview-explainable code
- Async EF queries
- Basic structured logging
- Migration-based schema
- Swagger docs for all endpoints
- Health check endpoint (`/health`)

## 9. Security Requirements
- Hash passwords (ASP.NET PasswordHasher or BCrypt)
- JWT signing key from config/secrets (never hardcode real secrets)
- `[Authorize]` on protected endpoints
- Ownership checks in service layer
- No sensitive data in responses
- CORS configured for Angular dev client

## 10. Docker Requirements (MANDATORY)

### DR-1 Containers
- API must run in Docker container
- DB must run in separate container
- Use `docker-compose.yml` to orchestrate

### DR-2 Docker Files
- Provide:
  - `Dockerfile` for API (multi-stage build: SDK -> runtime)
  - `.dockerignore`
  - `docker-compose.yml` with at least:
    - `api`
    - `db`
- Expose API port (e.g., 8080 container -> 8080 host or 5000/8080 documented)

### DR-3 Configuration
- API uses environment variables from compose:
  - Connection string
  - JWT settings (issuer, audience, key)
  - ASP.NET Core environment
- No real secrets in repo; provide sample values only

### DR-4 Database Migrations in Docker
- Ensure DB schema is created via EF migrations.
- Preferred: apply migrations on API startup (safe retry) or documented command flow.

### DR-5 Developer Experience
- One-command startup:
  - `docker compose up --build`
- README must include:
  - how to run
  - how to stop
  - how to reset volumes/data
  - how to view logs

## 11. Acceptance Criteria
Backend is done when:
1. Register/login works and JWT protects endpoints.
2. Full CRUD for tasks and categories works.
3. User sees only own tasks/categories.
4. Task list supports pagination + search + category filter (combined).
5. Project follows required 4-layer architecture.
6. EF migrations create schema successfully.
7. Swagger is usable.
8. Errors are consistent and informative.
9. App + DB run successfully via Docker Compose.

## 12. Required Project Structure
- `TodoApp.sln`
- `TodoApp.Api`
- `TodoApp.Application`
- `TodoApp.Domain`
- `TodoApp.Infrastructure`

## 13. Required Deliverables
- Full source code
- Dockerfile, docker-compose.yml, .dockerignore
- EF migrations
- appsettings sample (no real secrets)
- README with run instructions (local + docker)
- Sample API requests (curl or Postman collection)

## 14. Definition of Done (Interview-Ready)
- Clean layered architecture + DI wiring
- Works both locally and in Docker
- Clear README with architecture and flow explanations
- Easy to demo end-to-end in 5–10 minutes