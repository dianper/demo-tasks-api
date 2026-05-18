# Tasks API — AI-Driven Pipeline Demo

> Demo repository showcasing a fully automated software delivery pipeline with GitHub Copilot, GitHub Actions, and ArgoCD.

---

## Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 Minimal API |
| Database | PostgreSQL + EF Core |
| Auth | JWT Bearer |
| Tests | xUnit · FluentAssertions · Testcontainers |
| Container | Docker (GHCR) |
| Deploy | Kubernetes + ArgoCD |

---

## How the AI Pipeline Works

```
Issue (label: ai)
      ↓
GitHub Action fires → assigns @copilot
      ↓
Copilot Agent reads issue + copilot-instructions.md
      ↓
Agent implements → creates branch → writes tests → opens PR
      ↓
CI runs (build + unit + integration tests)
      ↓
AI reviews PR
      ↓
⛔ HUMAN GATE: engineer approves PR
      ↓
Merge → CD builds Docker image → pushes to GHCR
      ↓
repository_dispatch → k8s-assets repo opens PR with new image tag
      ↓
⛔ HUMAN GATE: engineer approves infra PR
      ↓
ArgoCD syncs → Kubernetes rolling update
```

---

## Setup Guide

### 1. Repository Settings

In your GitHub repo → **Settings → General → Pull Requests**:
- ✅ Allow auto-merge
- ✅ Automatically delete head branches after merge

### 2. Branch Protection on `main`

Go to **Settings → Branches → Add rule** for `main`:
- ✅ Require a pull request before merging
- ✅ Require status checks to pass: `Build & Test (.NET 10)`
- ✅ Require branches to be up to date before merging
- ✅ Do not allow bypassing the above settings

### 3. Enable GitHub Copilot Coding Agent

Requires **GitHub Copilot Pro+** or **Copilot Enterprise**.

Go to **Settings → Copilot → Coding agent** → Enable.

The agent will read `.github/copilot-instructions.md` automatically.

### 4. Create the `ai` Label

Go to **Issues → Labels → New label**:
- Name: `ai`
- Color: `#0075ca`
- Description: `Activate AI agent for this issue`

### 5. Secrets Required

Go to **Settings → Secrets and variables → Actions**:

| Secret | Description |
|---|---|
| `GH_GITOPS_PAT` | GitHub PAT with `repo` scope for the k8s-assets repo |

The `GITHUB_TOKEN` is automatic — no setup needed.

### 6. (Optional) K8s Assets Repo

If you have a `k8s-assets` repo for GitOps, add this workflow there:

```yaml
# k8s-assets/.github/workflows/update-image.yml
on:
  repository_dispatch:
    types: [new-image-available]

jobs:
  update-manifest:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Update image tag
        run: |
          SERVICE=${{ github.event.client_payload.service }}
          NEW_TAG=${{ github.event.client_payload.tag }}
          # Update Helm values or Kustomize overlay
          sed -i "s|tasks-api:v.*|tasks-api:${NEW_TAG}|g" \
            services/tasks-api/values.yaml
      - name: Open PR
        uses: peter-evans/create-pull-request@v6
        with:
          title: "chore(tasks-api): update image to ${{ github.event.client_payload.tag }}"
          branch: "chore/tasks-api-${{ github.event.client_payload.tag }}"
```

---

## Running Locally

```bash
# Requirements: .NET 10 SDK, Docker (for Testcontainers)

# Restore
dotnet restore

# Build
dotnet build

# Run unit tests
dotnet test tests/TasksApi.UnitTests

# Run integration tests (starts PostgreSQL via Testcontainers)
dotnet test tests/TasksApi.IntegrationTests

# Run the API (needs a local PostgreSQL or connection string)
cd src/TasksApi
dotnet run
```

---

## Creating an AI-Powered Issue

1. Go to **Issues → New Issue**
2. Write a clear description with acceptance criteria
3. Add the label **`ai`**
4. Submit — the pipeline starts automatically

**Example issue:**

```
Title: Add due date field to tasks

## Description
Tasks should support an optional due date so users can track deadlines.

## Acceptance Criteria
- [ ] POST /tasks accepts an optional `dueDate` (ISO 8601)
- [ ] GET /tasks/{id} returns `dueDate` in the response
- [ ] Tasks can be filtered by due date via GET /tasks?dueDate=2025-12-31
- [ ] Due date cannot be in the past (validation)

## Notes
- Use DateOnly for the domain type
- Add a DB index on due_date column
```

---

## Project Structure

```
.github/
  copilot-instructions.md   # AI agent global context
  skills/
    backend.md              # .NET patterns and examples
    testing.md              # xUnit + Testcontainers conventions
    git-workflow.md         # Branch, commit, PR rules
  workflows/
    ai-trigger.yml          # Listens for label "ai" → activates Copilot
    ci.yml                  # Build + tests on every PR
    cd.yml                  # Docker build + push after merge to main
  pull_request_template.md

src/TasksApi/
  Endpoints/
  Features/
    Tasks/
      Commands/
      Queries/
  Domain/
  Infrastructure/
  Auth/
  Common/
  Program.cs

tests/
  TasksApi.UnitTests/
  TasksApi.IntegrationTests/

Dockerfile
```