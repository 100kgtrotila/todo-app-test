# Conventions

## Language / runtime
- .NET 8
- C# latest stable supported by SDK in repo

## Naming
- C#: PascalCase types/methods/properties
- Private fields: `_camelCase`
- Interfaces: `I*`
- CQRS:
  - `*Command`, `*Query`
  - `*Handler`
  - `*Request`, `*Dto`, `*Response`
- PostgreSQL schema: snake_case (tables/columns/keys/indexes)

## API conventions
- Routes: `/api/<resource>`
- Status codes:
  - 200 OK (read/update)
  - 201 Created (create)
  - 204 NoContent (delete)
  - 400/401/403/404/409 for expected failures
- DTOs only across API boundary (no EF entity exposure)

## Async conventions
- Use async EF APIs.
- If method only returns one Task call, return Task directly (no redundant `async/await`).
- Always pass `CancellationToken` in async operations.

## Validation conventions
- Validate request DTOs.
- Reject invalid IDs/ownership violations deterministically.
- Keep validation messages consistent.

## Query conventions
- Keep filters SQL-translatable.
- Pagination must always include:
  - page
  - pageSize
  - totalCount
  - items

## Date/time conventions
- UTC only for persisted timestamps.
- `created_at` immutable after insert.
- `updated_at` changes on update.

## Documentation conventions
If behavior/contract/schema changes, update:
- `README.md` (public behavior)
- `docs/API_EXAMPLES.http` (verification requests)
- migration notes (if schema changed)