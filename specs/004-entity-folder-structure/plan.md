# Implementation Plan: Entity Folder Structure Reorganization

**Branch**: `004-entity-folder-structure` | **Date**: April 22, 2026 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/004-entity-folder-structure/spec.md`

## Summary

Reorganize the project's entity files from a flat `Entities/` root directory into a two-level hierarchy: `Models/Entities/<PluralName>/`. The three existing entities (User, Role, UserRole) each move into their own plural-named subfolder under `Models/Entities/`. All four files that consume these entities via `using Markwell.Core.Entities;` are updated to the new namespace. No new behavior, no new tests required — success criterion is a clean build.

## Technical Context

**Language/Version**: C# 13, .NET 10.0 LTS  
**Primary Dependencies**: ASP.NET Core Identity (entities inherit from IdentityUser/IdentityRole/IdentityUserRole)  
**Storage**: EF Core 10.0 — `StorageBroker : IdentityDbContext` (entity types registered there)  
**Testing**: xUnit — no new tests required (refactoring only; zero behavioral change)  
**Target Platform**: .NET 10.0 on Windows/Linux  
**Project Type**: Web API — single project (`Markwell.Core`)  
**Performance Goals**: N/A — pure structural refactoring  
**Constraints**: Project MUST compile with zero errors and zero warnings after all moves  
**Scale/Scope**: 3 entity files moved + 4 consumer `using` directives updated

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

✅ **PASS**: All constitution principles verified:

1. **Naming Conventions**: `Models/Entities/` aligns with constitution "Models: no suffix". Plural subfolder names (`Users`, `Roles`, `UserRoles`) match the Controllers plural convention extended to folder grouping. No file or class names change.

2. **Layered Architecture**: No layer boundary changes. Entities remain consumed only by Brokers; the dependency direction Controller → Service → Broker → Entity is unaffected.

3. **Method Design**: No methods added or changed.

4. **Code Clarity**: No new comments required. Namespace update is a mechanical rename.

5. **Testing Discipline**: No new behavior introduced — no new unit or integration tests required for this feature. The build passing is the test.

6. **Development Workflow**: Spec-kit workflow followed. Feature branch `004-entity-folder-structure` created.

**Status**: ✅ GATE PASSED — No constitution violations. Proceed to Phase 0.

---

## Project Structure

### Documentation (this feature)

```text
specs/004-entity-folder-structure/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit-tasks — NOT created here)
```

### Source Code — Before and After

**Before** (current):

```text
Markwell.Core/
├── Entities/
│   ├── User.cs           (namespace: Markwell.Core.Entities)
│   ├── Role.cs           (namespace: Markwell.Core.Entities)
│   └── UserRole.cs       (namespace: Markwell.Core.Entities)
├── Brokers/
│   ├── IProfileBroker.cs (using Markwell.Core.Entities)
│   ├── ProfileBroker.cs  (using Markwell.Core.Entities)
│   └── StorageBroker.cs  (using Markwell.Core.Entities)
└── Data/
    └── RoleSeeder.cs     (using Markwell.Core.Entities)
```

**After** (target):

```text
Markwell.Core/
├── Models/
│   └── Entities/
│       ├── Users/
│       │   └── User.cs       (namespace: Markwell.Core.Models.Entities)
│       ├── Roles/
│       │   └── Role.cs       (namespace: Markwell.Core.Models.Entities)
│       └── UserRoles/
│           └── UserRole.cs   (namespace: Markwell.Core.Models.Entities)
├── Brokers/
│   ├── IProfileBroker.cs     (using Markwell.Core.Models.Entities)
│   ├── ProfileBroker.cs      (using Markwell.Core.Models.Entities)
│   └── StorageBroker.cs      (using Markwell.Core.Models.Entities)
└── Data/
    └── RoleSeeder.cs         (using Markwell.Core.Models.Entities)
```

**Structure Decision**: Single-project extension. New directory tree `Models/Entities/<PluralName>/` created inside existing `Markwell.Core/`. Old `Entities/` directory deleted after all files are moved.

---

## Complexity Tracking

No constitution violations requiring justification. Feature is a straightforward structural rename.

---

## Phase 0: Outline & Research

### Unknowns Identified

**U1**: Should entity namespaces mirror the full folder depth (`Markwell.Core.Models.Entities.Users`) or use a shared flat namespace (`Markwell.Core.Models.Entities`)?

**U2**: Does `Program.cs` or any file not caught by `grep` reference `Markwell.Core.Entities` directly?

### Research Findings → research.md

*(See research.md for full decision log)*

---

## Phase 1: Design & Contracts

### Namespace Convention (from research.md)

All three entity files use the same flat namespace: **`Markwell.Core.Models.Entities`**

Rationale: consumer files remain on a single `using` directive. Granular per-entity namespaces (`Markwell.Core.Models.Entities.Users`) add verbosity with no type-safety benefit — all entity names in the project are already unique.

### Entity File Locations

| Entity | Old Path | New Path | New Namespace |
|--------|----------|----------|---------------|
| User | `Entities/User.cs` | `Models/Entities/Users/User.cs` | `Markwell.Core.Models.Entities` |
| Role | `Entities/Role.cs` | `Models/Entities/Roles/Role.cs` | `Markwell.Core.Models.Entities` |
| UserRole | `Entities/UserRole.cs` | `Models/Entities/UserRoles/UserRole.cs` | `Markwell.Core.Models.Entities` |

### Consumer Files — `using` Directive Update

| File | Change |
|------|--------|
| `Brokers/IProfileBroker.cs` | `using Markwell.Core.Entities;` → `using Markwell.Core.Models.Entities;` |
| `Brokers/ProfileBroker.cs` | `using Markwell.Core.Entities;` → `using Markwell.Core.Models.Entities;` |
| `Brokers/StorageBroker.cs` | `using Markwell.Core.Entities;` → `using Markwell.Core.Models.Entities;` |
| `Data/RoleSeeder.cs` | `using Markwell.Core.Entities;` → `using Markwell.Core.Models.Entities;` |

### Contracts

No external interface contracts are affected. This is a purely internal structural change — no public API signatures, no endpoint paths, no broker interface method signatures change.

### No data-model.md Required

No new entities are introduced. The three existing entities retain all their fields, relationships, and Identity base class inheritance. A data-model.md is not produced for this feature.

---

## Plan Approval

| Item | Status | Sign-Off |
|---|---|---|
| Constitution Check | ✅ PASS | No violations |
| Technical Context | ✅ VERIFIED | All paths confirmed from codebase scan |
| Architecture Design | ✅ READY | Flat namespace, per-entity subfolder pattern chosen |
| Structure Plan | ✅ APPROVED | 3 files moved, 4 consumer files updated, old directory deleted |
| Phases | ✅ OUTLINED | Phase 0 (research), Phase 1 (design), Phase 2 (tasks) mapped |

**Plan Status**: ✅ **READY FOR PHASE EXECUTION**
