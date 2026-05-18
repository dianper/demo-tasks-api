# Copilot Instructions — Tasks API

You are an AI agent working on a production .NET 10 Web API.
Read this file fully before touching any code.

---

## Project Overview

**Name:** Tasks API  
**Purpose:** REST API for task management (create, assign, complete tasks)  
**Stack:** .NET 10 · Minimal API · PostgreSQL · Entity Framework Core · JWT Auth  
**Test frameworks:** xUnit · FluentAssertions · Testcontainers (integration)  

---

## Repository Structure

```
src/TasksApi/
  ├── Endpoints/        # Minimal API route registration (one file per resource)
  ├── Features/         # Business logic — one folder per feature (CQRS-lite)
  │   └── Tasks/
  │       ├── Commands/
  │       └── Queries/
  ├── Domain/           # Entities, enums, value objects (no dependencies)
  ├── Infrastructure/   # EF Core DbContext, repositories, migrations
  ├── Auth/             # JWT configuration, token service
  ├── Common/           # Result<T>, Error types, extensions
  └── Program.cs        # App entry point — keep minimal

tests/
  ├── TasksApi.UnitTests/         # Pure unit tests, no I/O
  └── TasksApi.IntegrationTests/  # Testcontainers (real PostgreSQL)
```

---

## Architecture Rules

- **Minimal API only** — no Controllers. Routes go in `Endpoints/` as extension methods.
- **CQRS-lite** — separate Commands and Queries under `Features/`. No MediatR.
- **Result<T> pattern** — never throw exceptions for business logic. Return `Result<T>`.
- **No logic in endpoints** — endpoints only validate input, call a feature handler, map to response.
- **Domain is pure** — entities in `Domain/` have zero infrastructure dependencies.
- **EF Core** — use `AppDbContext` directly in handlers (no generic repository).
- **Async everywhere** — all handlers and endpoints must be `async Task<T>`.

---

## Coding Conventions

- Language: **C# 13**, nullable enabled, implicit usings enabled
- Naming:
  - Commands: `CreateTaskCommand`, `CompleteTaskCommand`
  - Queries: `GetTaskByIdQuery`, `ListTasksQuery`
  - Handlers: `CreateTaskHandler`, `GetTaskByIdHandler`
  - Endpoints: `TaskEndpoints` (static class with `MapTaskEndpoints(this WebApplication app)`)
- DTOs: suffix with `Request` (input) and `Response` (output), defined next to the feature
- Error codes: use `Error` record in `Common/` — e.g. `Error.NotFound("Task.NotFound", "Task not found")`
- Never use `var` when the type is not obvious from the right-hand side

---

## JWT Auth

- All endpoints require JWT by default unless explicitly marked `.AllowAnonymous()`
- Public endpoints: `POST /auth/token` only
- Claims used: `sub` (userId), `email`, `role`
- Roles: `Admin`, `User`
- Use `[Authorize(Roles = "Admin")]` for admin-only endpoints

---

## Database & Migrations

- PostgreSQL via EF Core
- Connection string key: `ConnectionStrings__DefaultConnection`
- When adding a new entity:
  1. Create entity in `Domain/`
  2. Add `DbSet<T>` to `AppDbContext`
  3. Configure via Fluent API in `AppDbContext.OnModelCreating`
  4. Run `dotnet ef migrations add <MigrationName> -p src/TasksApi`
- Never use data annotations on entities — use Fluent API only

---

## Testing Rules

See `.github/skills/testing.md` for full conventions.

**Summary:**
- Unit tests: test handlers in isolation, mock `AppDbContext` with `MockDbContext` helper
- Integration tests: use `TasksApiFactory` (Testcontainers) — spins up real PostgreSQL
- Every new endpoint needs at least: happy path + 401 unauthorized + 404/400 where applicable
- Test method naming: `MethodName_Scenario_ExpectedResult`

---

## Git & PR Workflow

See `.github/skills/git-workflow.md` for full conventions.

**Summary:**
- Branch: `feature/issue-{number}-{slug}` or `fix/issue-{number}-{slug}`
- Commits: conventional commits (`feat:`, `fix:`, `test:`, `chore:`)
- PR title must reference issue: `feat: add task priority field (#42)`
- PR description must include: Summary, How to test, Checklist

---

## Skill Usage

- For implementation patterns (endpoints, handlers, Result<T>, domain, EF mappings), consult `.github/skills/backend.md` before coding.
- For test strategy and conventions (unit/integration coverage, naming, fixtures), consult `.github/skills/testing.md` when adding or changing tests.
- For branching, commit messages, and PR workflow, consult `.github/skills/git-workflow.md` before Git operations.

---

## When Implementing a New Feature

1. Read the issue carefully. If requirements are ambiguous, note assumptions in the PR description.
2. Consult `.github/skills/backend.md` for implementation patterns and constraints.
3. Create branch from `main` following `.github/skills/git-workflow.md` naming rules.
4. Implement in this order: Domain → Infrastructure → Feature handler → Endpoint → Tests.
5. Consult `.github/skills/testing.md` and add/adjust tests for the feature.
6. Run `dotnet build` and `dotnet test` locally before opening PR.
7. Open PR with the template in `.github/pull_request_template.md`.
8. Do NOT merge — a human engineer will review and approve.

---

## What You Must NOT Do

- Do not add NuGet packages without listing them in the PR description with justification
- Do not modify `Program.cs` structure — only add `app.MapXEndpoints()` calls
- Do not delete or modify existing migrations
- Do not commit secrets, connection strings, or API keys
- Do not use `Thread.Sleep` or `.Result` / `.Wait()` — async only
- Do not bypass JWT auth on existing protected endpoints