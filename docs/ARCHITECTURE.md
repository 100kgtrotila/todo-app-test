# Architecture Overview

## High-level request flow
1. `Controller` receives HTTP request and user context (JWT claims).
2. Controller sends command/query via MediatR (`ISender`).
3. `Application` handler executes use-case logic.
4. Handler calls repository interfaces.
5. `Infrastructure` repositories use EF Core (`AppDbContext`) against PostgreSQL.
6. Handler maps entities -> DTO.
7. Controller returns response.

## Layers and responsibilities

### TodoApp.Api
- Controllers
- Auth/JWT setup
- Swagger
- Global exception pipeline
- DI composition root

### TodoApp.Application
- CQRS handlers (features)
- DTOs, request models
- contracts (`I*Repository`, service abstractions)
- application-level errors/result mapping policy

### TodoApp.Domain
- Entities (`User`, `TaskItem`, `Category`, etc.)
- Core invariants (if any)

### TodoApp.Infrastructure
- `AppDbContext`
- EF configurations
- repository implementations
- technical services (token/password hashing)

## Key architectural constraints
- Controllers must stay thin.
- Business rules must not live in controllers.
- Application must not depend on Infrastructure concrete types.
- Domain must not depend on EF or ASP.NET.

## Security model
- JWT auth for protected endpoints.
- Ownership checks in application logic: users can only access their own resources.

## Data model conventions
- PostgreSQL + snake_case naming.
- UTC for timestamps.
- Migrations-driven schema evolution.

## Error handling model
- Expected business errors: explicit and mapped to stable HTTP status codes.
- Unexpected failures: global exception handler + ProblemDetails.