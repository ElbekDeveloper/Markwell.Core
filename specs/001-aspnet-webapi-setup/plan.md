# Implementation Plan: ASP.NET Core Web API Project Setup

**Branch**: `001-aspnet-webapi-setup` | **Date**: 2026-04-16 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-aspnet-webapi-setup/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Create a minimal, production-ready ASP.NET Core Web API project targeting the latest stable .NET runtime. The project must compile and run without errors, contain no boilerplate endpoints or template comments, and be submitted via a reviewed pull request linked to a GitHub issue. The setup enforces the project constitution by using Scalar for manual API testing during development.

## Technical Context

**Language/Version**: C# 13 (.NET 9 LTS or latest stable)  
**Primary Dependencies**: ASP.NET Core (latest stable), .NET SDK  
**Storage**: N/A — Foundation-layer feature with no persistence requirement  
**Testing**: xUnit + Fluent Assertions (standard for .NET Web APIs)  
**Target Platform**: .NET Core / .NET 9+  
**Project Type**: web-service (REST API backend)  
**Performance Goals**: Sub-100ms startup, sub-50ms health check response  
**Constraints**: Minimal footprint; no external dependencies beyond ASP.NET Core framework; clean Program.cs with zero template comments  
**Scale/Scope**: Single Web API project; foundation for future domain features; manual testing via Scalar API documentation UI

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

✅ **Layered Architecture**: Not applicable to foundation setup; will be enforced in subsequent features (Broker → Service → Controller).  
✅ **Code Clarity**: Program.cs will contain zero template comments; minimal comments only for non-obvious configuration.  
✅ **Naming Conventions**: Project name follows PascalCase. Controllers (when added later) will follow `*sController` naming.  
✅ **Testing Discipline**: Foundation layer; unit tests deferred to feature-specific implementations. Integration tests will verify API startup health.  
✅ **Git Discipline**: Feature branch workflow enforced; pull request required; GitHub issue created beforehand (ref: constitution § Development Workflow).  

**GATE RESULT**: ✅ PASS — All constitution principles compatible with foundation setup scope.

## Project Structure

### Documentation (this feature)

```text
specs/001-aspnet-webapi-setup/
├── spec.md              # Feature specification
├── plan.md              # This file (implementation plan)
├── research.md          # Phase 0 output (design decisions and rationale)
├── data-model.md        # Phase 1 output (entities and validation contracts)
├── quickstart.md        # Phase 1 output (developer onboarding guide)
└── contracts/           # Phase 1 output (REST API contract)
    └── health-check.contract.md
```

### Source Code (repository root)

```text
Markwell.Core/
├── Markwell.Core.csproj         # Project file (C# 13, .NET 9 LTS)
├── Program.cs                   # Clean entry point (zero comments)
├── appsettings.json             # Default configuration
├── appsettings.Development.json # Dev-only settings
└── Properties/
    └── launchSettings.json      # IIS Express / Kestrel launch config
```

**Structure Decision**: Single ASP.NET Core Web API project with minimal startup configuration. No layered folders yet (models/, services/, brokers/); these will be created as domain features are added. Scalar integration enables interactive API documentation for manual testing without requiring external tools.

## Phase 0: Research & Clarification

**Status**: ✅ COMPLETE — All technical choices pre-determined by architecture standards and specification.

### Key Decisions

| Decision | Rationale | Alternatives Considered |
|----------|-----------|-------------------------|
| **ASP.NET Core + .NET 9 LTS** | Official, stable, enterprise-grade; latest LTS ensures support and feature parity | .NET 8, Node.js (not C#/.NET) |
| **Minimal Program.cs** | Spec requirement (zero comments, no boilerplate); clean foundation for constitution compliance | Full ASP.NET Core template (rejected) |
| **No sample endpoints** | Spec requirement (FR-004); developers will add domain-specific endpoints as features progress | WeatherForecast controller (template default, rejected) |
| **Scalar for API testing** | User preference; lightweight, OpenAPI-native UI for interactive manual testing during development | Swagger UI (heavier), Postman (external) |
| **xUnit + Fluent Assertions** | .NET community standard; integrates cleanly with ASP.NET Core; supports test-first discipline | NUnit, MSTest (less idiomatic) |
| **Single project layout** | Foundation feature; no layering yet. Domain separation (Broker/Service/Controller) enforced in subsequent features | Multi-project structure (premature) |

**Unknowns resolved**: None. All technical context is deterministic.

---

## Phase 1: Design & Contracts

### 1.1 Data Model

Since this is a **foundation-layer feature** with no domain logic, there are no persistent entities or domain models at this stage. The data model consists of:

- **Health Check Response**: JSON structure for the foundation's liveness probe.
  ```json
  {
    "status": "healthy",
    "timestamp": "2026-04-16T10:30:00Z",
    "version": "1.0.0"
  }
  ```

- **Error Response** (standard): Applied to any API endpoints added in the future.
  ```json
  {
    "error": "description",
    "statusCode": 400,
    "timestamp": "2026-04-16T10:30:00Z"
  }
  ```

**Output**: `data-model.md` (see Phase 1 artifacts below)

### 1.2 Public API Contract

The foundation API exposes a **Health Check endpoint** for orchestration, monitoring, and deployment verification:

**Endpoint**: `GET /health`

**Purpose**: Liveness probe; allows infrastructure/orchestration tools to verify the API is running and healthy.

**Response** (200 OK):
```json
{
  "status": "healthy",
  "timestamp": "2026-04-16T10:30:00Z",
  "version": "1.0.0"
}
```

**Output**: `contracts/health-check.contract.md`

### 1.3 Quickstart Guide

**Output**: `quickstart.md` — Covers:
- Project prerequisites (.NET SDK version)
- Clone, build, run instructions
- Verify startup (curl /health)
- Manual testing via Scalar UI
- Next steps (adding domain features)

### 1.4 Agent Context Update

Run: `.specify/scripts/powershell/update-agent-context.ps1 -AgentType claude`
- Adds ASP.NET Core 9 + C# 13 to technology stack
- Documents clean architecture principles
- Links to constitution

---

## Phase 2: Task Generation

**Status**: To be executed via `/speckit-tasks` command.

**Deliverable**: `tasks.md` with dependency-ordered task list covering:
1. Create ASP.NET Core project (`dotnet new webapi`)
2. Clean Program.cs (remove template comments and boilerplate)
3. Add Health Check endpoint (Minimal API or controller)
4. Configure Scalar API documentation UI
5. Create GitHub issue (before PR)
6. Create pull request targeting `master`
7. Verify PR links issue
8. Merge workflow validation

**Complexity**: Low — Linear task sequence; no interdependencies beyond sequential order.

---

## Summary Table

| Artifact | Status | Purpose |
|----------|--------|---------|
| `spec.md` | ✅ Complete | Feature requirements and acceptance criteria |
| `plan.md` | ✅ Complete (this file) | Design decisions and architecture |
| `research.md` | To Generate | Research findings (if needed; currently minimal) |
| `data-model.md` | To Generate | Health check schema and error contracts |
| `contracts/health-check.contract.md` | To Generate | OpenAPI-compatible health endpoint spec |
| `quickstart.md` | To Generate | Developer setup and verification steps |
| `tasks.md` | To Generate (next: `/speckit-tasks`) | Implementation task checklist |

**Next Step**: Proceed to Phase 2 execution via `/speckit-tasks` to generate the task checklist.
