---
name: "Backend Feature (AI)"
description: "Request a new backend feature to be implemented by the AI agent"
title: "[Feature]: "
labels: ["ai", "backend", "feature"]
assignees: []
---

## 📋 Feature Description
<!-- A clear and concise description of the feature. What should this API do? -->


---

## 🎯 Acceptance Criteria
<!-- The AI agent will use these as the definition of done. Be specific. -->

- [ ] 
- [ ] 
- [ ] 

---

## 🌐 Endpoints

<!-- List the HTTP endpoints this feature requires. Add/remove rows as needed. -->

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| `GET` | `/` | 🔒 JWT | |
| `POST` | `/` | 🔒 JWT | |

---

## 📦 Request / Response Shape

<!-- Describe the expected JSON bodies. The agent will derive the DTOs from this. -->

**Request:**
```json
{
  
}
```

**Response:**
```json
{
  
}
```

---

## 🗃️ Database Changes

<!-- Does this feature require schema changes? -->

- [ ] No DB changes needed
- [ ] New table / entity: `<!-- name -->`
- [ ] New columns on existing table: `<!-- table.column -->`
- [ ] New index on: `<!-- table.column -->`
- [ ] Migration required

---

## ✅ Validation Rules

<!-- List all input validation rules the agent must enforce. -->

| Field | Rule |
|-------|------|
| | |

---

## 🔐 Authorization

<!-- Who can access these endpoints? -->

- [ ] Any authenticated user (`User` role)
- [ ] Admin only (`Admin` role)
- [ ] Owner only (user can only access their own resources)
- [ ] Public (no auth required — justify below)

---

## ⚠️ Business Rules

<!-- Any domain logic the agent must implement. Edge cases, constraints, etc. -->

- 

---

## 📎 Notes & Context

<!-- Additional context, links to docs, related issues, design decisions, etc. -->


---

## 🚫 Out of Scope

<!-- Explicitly list what the agent should NOT implement in this issue. -->

-