# Skill: Git Workflow

Reference this file when creating branches, commits, and pull requests.

---

## Branch Naming

```
feature/issue-{number}-{short-slug}
fix/issue-{number}-{short-slug}
chore/issue-{number}-{short-slug}
```

Examples:
```
feature/issue-12-add-task-priority
fix/issue-34-complete-task-status-bug
chore/issue-56-update-ef-core
```

Rules:
- Always branch from `main`
- Slug: lowercase, hyphens only, max 5 words
- Never work directly on `main`

---

## Commit Convention

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <description>

[optional body]
```

Types:
| Type | When to use |
|---|---|
| `feat` | New feature or endpoint |
| `fix` | Bug fix |
| `test` | Adding or fixing tests |
| `refactor` | Code change without behaviour change |
| `chore` | Dependencies, config, migrations |
| `docs` | Documentation only |

Examples:
```
feat(tasks): add POST /tasks endpoint with JWT auth
test(tasks): add integration tests for task creation
fix(tasks): return 404 instead of 500 when task not found
chore: add EF Core migration for task priority field
```

Rules:
- One logical change per commit
- Present tense, lowercase, no period at end
- Body optional but encouraged for non-trivial changes

---

## Pull Request

**Title format:**
```
feat: add task priority field (#42)
fix: correct task status on completion (#51)
```

**PR Description Template** (also at `.github/pull_request_template.md`):

```markdown
## Summary
<!-- What does this PR do? Why? -->

## Changes
- [ ] Domain: added/modified entity
- [ ] Feature: handler implemented
- [ ] Endpoint: route registered
- [ ] Tests: unit + integration added

## New NuGet Packages
<!-- List any new packages with justification, or "None" -->

## How to Test
<!-- Steps to manually verify this works -->

## Checklist
- [ ] `dotnet build` passes
- [ ] `dotnet test` passes
- [ ] No secrets or connection strings committed
- [ ] No `.Result` or `.Wait()` used
- [ ] Branch created from `main`
```

---

## Rules for the AI Agent

1. **Always** create a branch before touching any file
2. **Never** commit to `main` directly
3. **Never** force-push
4. Open PR as **Draft** first, then mark as Ready when all checks pass
5. After opening PR, post a comment with a brief summary of decisions made
6. If requirements were ambiguous, document assumptions in the PR description
7. Request review from the human engineer after CI passes