# Research: Entity Folder Structure Reorganization

**Feature**: `004-entity-folder-structure` | **Phase**: 0 | **Date**: April 22, 2026

---

## U1 — Namespace Convention: Flat vs. Mirrored

**Question**: Should entity namespaces mirror the full folder depth (`Markwell.Core.Models.Entities.Users`) or use a shared flat namespace (`Markwell.Core.Models.Entities`)?

**Decision**: Flat namespace — `Markwell.Core.Models.Entities` for all three entities.

**Rationale**:
- All 4 consumer files currently use a single `using Markwell.Core.Entities;`. A flat namespace preserves this one-directive pattern.
- Entity class names (`User`, `Role`, `UserRole`) are globally unique in the project — no disambiguation via namespace depth is needed.
- Deep namespaces (`Markwell.Core.Models.Entities.Users`) would require three separate `using` directives in any file that consumes more than one entity, adding noise without benefit.
- .NET projects commonly use logical namespaces that group by concept rather than exactly mirroring folder depth (e.g., `Microsoft.EntityFrameworkCore` spans many subdirectories internally).

**Alternatives Considered**:
- **Full mirror** (`Markwell.Core.Models.Entities.Users`) — rejected: consumer files would need multiple using directives; no type-safety benefit since entity names are unique.
- **Keep old namespace** (`Markwell.Core.Entities`) with only folder renamed — rejected: misleads readers whose IDEs show namespace hierarchy; namespace should reflect the new location logically.

---

## U2 — Complete Consumer File Scan

**Question**: Does `Program.cs` or any other file reference `Markwell.Core.Entities` outside the `Entities/` folder?

**Decision**: No additional files found beyond the four already identified.

**Findings** (from `grep -rn "Markwell.Core.Entities" --include="*.cs"`):
```
Brokers/IProfileBroker.cs:1:  using Markwell.Core.Entities;
Brokers/ProfileBroker.cs:2:   using Markwell.Core.Entities;
Brokers/StorageBroker.cs:4:   using Markwell.Core.Entities;
Data/RoleSeeder.cs:3:         using Markwell.Core.Entities;
```

- `Program.cs` — no entity namespace usage (Identity types referenced via generic type params, not direct using)
- `Tests/` — no entity namespace usage (no test files exist yet)
- No string-literal paths referencing `Entities/` found in `.cs` files

**Total files requiring `using` update**: 4

---

## Summary of Decisions

| Decision | Choice | Rationale |
|---|---|---|
| Namespace | `Markwell.Core.Models.Entities` (flat) | Single using directive per consumer file; entity names are unique |
| Consumer files | 4 files (IProfileBroker, ProfileBroker, StorageBroker, RoleSeeder) | Confirmed by full codebase scan |
| tests | No new tests | Pure rename; zero behavioral change |
| Contracts | No changes | Internal refactoring; no public API affected |
