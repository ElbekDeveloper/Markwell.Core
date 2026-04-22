# Feature Specification: Entity Folder Structure Reorganization

**Feature Branch**: `004-entity-folder-structure`  
**Created**: April 22, 2026  
**Status**: Draft  
**Input**: "Rename Entities to Models, with an Entities subfolder inside for DB-mapped models. Each entity lives in its own named subfolder."

## User Scenarios & Testing

### User Story 1 - Rename Root Entities Directory to Models (Priority: P1)

The development team renames the top-level `Entities/` directory to `Models/` and establishes an `Entities/` subdirectory inside it. This gives the project a clear, extensible home for all model types: database entities go in `Models/Entities/`, and future model types (DTOs, request/response objects, view models) can be added as siblings without ambiguity.

**Why this priority**: This is the structural foundation. All subsequent reorganization depends on this rename being in place first. Without it, the folder hierarchy makes no distinction between database entities and other model types.

**Independent Test**: Verifiable by confirming `Models/Entities/` exists at the project root, the old `Entities/` root directory no longer exists, and the project builds without errors.

**Acceptance Scenarios**:

1. **Given** the project has a root-level `Entities/` directory, **When** the rename is applied, **Then** `Entities/` no longer exists at the root and `Models/Entities/` exists in its place
2. **Given** the renamed directory structure is in place, **When** a developer opens the project, **Then** `Models/` is the single top-level home for all model types
3. **Given** the rename is applied, **When** the project is built, **Then** it compiles with zero errors

---

### User Story 2 - Move Each Entity Into Its Own Named Subfolder (Priority: P1)

Each database-mapped entity class is moved into a dedicated subdirectory named after the entity's plural form. For example, `User.cs` moves to `Models/Entities/Users/`, `Role.cs` to `Models/Entities/Roles/`, and `UserRole.cs` to `Models/Entities/UserRoles/`.

**Why this priority**: Co-equal with US1 — moving files without establishing the structure first is meaningless, and establishing the structure without moving the files leaves it empty. Both must be delivered together to form an independently testable MVP.

**Independent Test**: Verifiable by confirming each entity file (`User.cs`, `Role.cs`, `UserRole.cs`) lives in its own plural-named subdirectory under `Models/Entities/`, and the project builds cleanly.

**Acceptance Scenarios**:

1. **Given** `User.cs` previously lived in `Entities/`, **When** the reorganization is applied, **Then** the file lives at `Models/Entities/Users/User.cs`
2. **Given** `Role.cs` previously lived in `Entities/`, **When** the reorganization is applied, **Then** the file lives at `Models/Entities/Roles/Role.cs`
3. **Given** `UserRole.cs` previously lived in `Entities/`, **When** the reorganization is applied, **Then** the file lives at `Models/Entities/UserRoles/UserRole.cs`
4. **Given** the files are moved, **When** any class in the codebase that referenced the old location is compiled, **Then** all references resolve correctly and no broken imports remain

---

### User Story 3 - Establish Convention for Future Entities (Priority: P2)

The folder structure establishes a clear, repeatable pattern. When a developer adds a new entity (e.g., `Course.cs`), the convention makes the correct destination unambiguous: `Models/Entities/Courses/Course.cs`.

**Why this priority**: P2 because it delivers ongoing value rather than a one-time fix. The convention is implicit in the structure created by P1, but it must be explicitly documented so developers follow it consistently without discussion.

**Independent Test**: Verifiable by adding a hypothetical entity description to the project's contributing guide and confirming the destination path can be determined mechanically from the entity name alone.

**Acceptance Scenarios**:

1. **Given** a new entity named `Course` needs to be added, **When** a developer follows the established convention, **Then** they create `Models/Entities/Courses/Course.cs` without needing to ask where it belongs
2. **Given** the convention is in place, **When** any future entity is added, **Then** its folder name is the plural form of the entity class name
3. **Given** a developer browses `Models/Entities/`, **When** they look for a specific entity, **Then** they can locate it by navigating to the folder that matches the entity's plural name

---

### Edge Cases

- What happens if a future entity has an irregular plural (e.g., `Person` → `People` vs `Persons`)? Use the grammatically correct English plural; document the choice in the entity's folder.
- What happens if two entities have the same plural folder name? Entity names must be unique in the domain model — this should not occur; reject at code review if it does.
- What happens to non-entity model files (DTOs, request/response objects) that may be added in future? They are out of scope for this feature; they may live in `Models/` siblings to `Entities/` (e.g., `Models/Requests/`, `Models/Responses/`) but that structure is defined in a future spec.
- What if a file in the codebase has a hardcoded path string referencing the old `Entities/` location? All such strings must be updated as part of this reorganization.

---

## Requirements

### Functional Requirements

- **FR-001**: The top-level `Entities/` directory MUST be renamed to `Models/`
- **FR-002**: An `Entities/` subdirectory MUST exist inside `Models/` as the home for all database-mapped entity classes
- **FR-003**: Each entity file MUST reside in its own subdirectory under `Models/Entities/`, named using the entity's plural form (e.g., `User.cs` → `Models/Entities/Users/User.cs`)
- **FR-004**: All namespace references, using directives, and any other references to moved files MUST be updated throughout the entire codebase to reflect the new locations
- **FR-005**: The project MUST compile with zero errors and zero warnings after the reorganization is complete
- **FR-006**: The naming convention for entity subfolders MUST use the plural form of the entity class name (e.g., entity `Role` → folder `Roles`)
- **FR-007**: No entity class file MAY reside directly in `Models/Entities/` — every entity MUST be inside its own named subfolder

### Key Entities

- **User**: Existing database-mapped entity; moves from `Entities/User.cs` → `Models/Entities/Users/User.cs`
- **Role**: Existing database-mapped entity; moves from `Entities/Role.cs` → `Models/Entities/Roles/Role.cs`
- **UserRole**: Existing database-mapped entity (junction); moves from `Entities/UserRole.cs` → `Models/Entities/UserRoles/UserRole.cs`

---

## Success Criteria

- **SC-001**: All three existing entity files (`User.cs`, `Role.cs`, `UserRole.cs`) reside in their respective plural-named subfolders under `Models/Entities/` after the reorganization
- **SC-002**: The project builds with zero compilation errors and zero warnings after all files are moved and references updated
- **SC-003**: No file at any path containing the old `Entities/` root location remains in the repository
- **SC-004**: A developer unfamiliar with the codebase can locate any entity by navigating `Models/Entities/<PluralEntityName>/` without searching
- **SC-005**: Adding a new entity to the project in the future requires no discussion about placement — the convention is self-evident from the existing structure

---

## Assumptions

- The project currently contains exactly three entity files: `User.cs`, `Role.cs`, and `UserRole.cs` — all in the root-level `Entities/` directory
- No `Models/` directory currently exists at the project root (confirmed: the root has `Entities/`, `Brokers/`, `Data/`, `Controllers/`, etc.)
- Namespace conventions in the project mirror the folder structure (standard .NET convention); updating the folder location therefore requires updating the namespace declaration inside each moved file
- All other files that reference moved entities (brokers, services, controllers, tests) import them by namespace and must have their `using` directives updated
- DTOs, request objects, and other non-DB model types are out of scope for this feature; they will be addressed in a future spec if needed
- The `Data/` directory (containing `RoleSeeder.cs`) is not affected by this reorganization
