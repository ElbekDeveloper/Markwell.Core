# Implementation Plan: Generic Storage Broker Integration

**Branch**: `003-storage-broker-integration` | **Date**: April 17, 2026 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/003-storage-broker-integration/spec.md`

## Summary

Implement a reusable, generic storage broker pattern for database access across Markwell.Core. The IStorageBroker interface defines standard CRUD operations (Insert, Select, SelectById, Update, Delete) that all brokers must implement. StorageBroker wraps ApplicationDbContext and reads database configuration from IConfiguration via dependency injection, eliminating configuration boilerplate from Program.cs. Model-specific brokers (RoleBroker, UserBroker, UserRoleBroker) implement the interface with one-to-one model mapping, supporting predefined roles (Admin, Manager, Teacher, Student) and enabling clean, consistent data access patterns throughout the application.

## Technical Context

**Language/Version**: C# 13, .NET 10.0 LTS  
**Primary Dependencies**: ASP.NET Core 10.0, EF Core 10.0, ASP.NET Core Identity  
**Storage**: EF Core with PostgreSQL (production) + SQLite (development/testing)  
**Testing**: xUnit for unit tests, Moq for mocking, SQLite in-memory for integration tests  
**Target Platform**: .NET 10.0 LTS on Linux or Windows servers  
**Project Type**: Web API service (single project)  
**Performance Goals**: 
- CRUD operations complete within 100ms for typical queries
- Support concurrent database access (DbContext thread-safety)

**Constraints**: 
- Connection string must be read from IConfiguration (no Program.cs configuration)
- Brokers must be registered with DI using single-line service registration
- Entity IDs are strings (compatible with Identity defaults)

**Scale/Scope**: 
- 3+ brokers (RoleBroker, UserBroker, UserRoleBroker)
- 4 predefined roles (Admin, Manager, Teacher, Student)
- Generic interface reusable for all future models

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

✅ **PASS**: All constitution principles verified:

1. **Naming Conventions**: Singular brokers (StorageBroker, RoleBroker), interface IStorageBroker with I prefix, PascalCase files, verb-based methods (Insert, Select, SelectById, Update, Delete)

2. **Layered Architecture**: Three-layer pattern maintained:
   - **Brokers**: IStorageBroker interface (abstract contract) + StorageBroker (concrete, no business logic)
   - **Model-Specific Brokers**: RoleBroker, UserBroker inherit/implement IStorageBroker
   - **Services & Controllers**: Consume brokers via interface only (dependency inversion)

3. **Method Design**: Generic methods with <T> parameters, verb-based names, parameter declarations within 120 chars

4. **Code Clarity**: XML documentation required for generic interface methods, `var` when type is clear

5. **Testing Discipline**: Unit tests required for all broker methods, integration tests verify DbContext interaction, mocks only at broker boundary

6. **Development Workflow**: Spec-kit workflow followed, feature branch `003-storage-broker-integration`, PR discipline required

**Status**: ✅ GATE PASSED - No constitution violations. Proceed to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/003-storage-broker-integration/
├── plan.md                # This file (/speckit.plan output)
├── research.md            # Phase 0 output (/speckit.plan command)
├── data-model.md          # Phase 1 output (/speckit.plan command)
├── quickstart.md          # Phase 1 output (/speckit.plan command)
├── contracts/             # Phase 1 output (/speckit.plan command)
│   └── broker-contract.md
├── checklists/
│   └── requirements.md
└── tasks.md               # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
Markwell.Core/
├── Program.cs                        # Updated: DI registration only
├── Markwell.Core.csproj             # Existing
├── Entities/
│   ├── User.cs                      # Existing (from US2)
│   ├── Role.cs                      # Existing (from US2)
│   └── UserRole.cs                  # Existing (from US2)
├── Models/
│   ├── CreateUserRequest.cs         # Existing (from US2)
│   ├── LoginRequest.cs              # Existing (from US2)
│   ├── [other DTOs]                 # Existing (from US2)
├── Services/
│   ├── UserService.cs               # Existing (from US2)
│   ├── RoleService.cs               # Existing (from US2)
│   ├── AuthenticationService.cs     # Existing (from US2)
│   └── ProfileManagementOrchestrationService.cs # Existing (from US2)
├── Brokers/
│   ├── IStorageBroker.cs            # NEW: Generic CRUD interface
│   ├── StorageBroker.cs             # NEW: DbContext implementation
│   ├── IdentityBroker.cs            # Existing (from US2)
│   ├── UserBroker.cs                # REFACTOR: Implement IStorageBroker
│   ├── RoleBroker.cs                # REFACTOR: Implement IStorageBroker
│   └── UserRoleBroker.cs            # NEW: Model-specific broker
├── Controllers/
│   ├── UsersController.cs           # Existing (from US2)
│   ├── AuthController.cs            # Existing (from US2)
├── Data/
│   ├── ApplicationDbContext.cs      # Existing (from US2)
├── Migrations/
│   └── [Auto-generated by EF Core]
├── appsettings.json                 # Existing: connection strings
├── appsettings.Development.json     # Existing: dev config
└── appsettings.Production.json      # Existing: prod config

Tests/
├── Markwell.Core.Tests.csproj
├── Unit/
│   ├── Brokers/
│   │   ├── StorageBrokerTests.cs    # NEW: Generic broker contract
│   │   ├── RoleBrokerTests.cs       # NEW: Role-specific tests
│   │   ├── UserBrokerTests.cs       # NEW: User-specific tests
│   │   └── UserRoleBrokerTests.cs   # NEW: UserRole-specific tests
│   └── [Services, Controllers]      # Existing (from US2)
└── Integration/
    ├── StorageBrokerIntegrationTests.cs   # NEW: DbContext interaction
    └── [Other integration tests]          # Existing (from US2)
```

**Structure Decision**: Single-project structure extends existing Markwell.Core Web API. New files created in Brokers/ directory for storage layer. No new project files required. DI registration centralized in Program.cs. All broker implementations follow constitution naming and layering principles. Model-specific brokers wrap generic StorageBroker to provide CRUD operations for each entity type.

## Complexity Tracking

| Item | Status | Notes |
|---|---|---|
| Generic interface pattern | Standard | IStorageBroker follows design patterns (Strategy, Template Method) commonly used in .NET brokers |
| DI configuration injection | Standard | IConfiguration built-in to ASP.NET Core; StorageBroker constructor injection standard pattern |
| Model-specific brokers | Standard | Each broker (RoleBroker, UserBroker) inherits/implements one interface, one-to-one mapping per constitution |
| Database multi-environment | Standard | Connection string switching via IConfiguration.GetConnectionString("DefaultConnection") |

**Complexity Assessment**: ✅ LOW - Standard patterns, no novel architecture. Constitution-aligned throughout.

---

## Next Steps: Phase Execution

### Phase 0: Outline & Research

**Goal**: Resolve any unknowns from Technical Context

**Unknowns to research** (from above):
- None identified. All technical choices are established (EF Core, Identity, DI patterns)

**Decisions verified**:
- ✅ Generic interface with <T> parameters fits C# 10+ generic constraints
- ✅ IConfiguration injection is standard ASP.NET Core pattern
- ✅ DbContext is thread-safe for concurrent CRUD (per EF Core documentation)
- ✅ Entity IDs as strings compatible with Identity defaults

**Output**: research.md (short; primarily confirms established decisions)

---

### Phase 1: Design & Contracts

**Goal**: Define broker interface, model relationships, API contracts

**Deliverables**:
1. **data-model.md**: Entity definitions (Role with predefined values)
2. **contracts/broker-contract.md**: IStorageBroker method signatures, CRUD contract
3. **quickstart.md**: Setup guide for developers using brokers
4. **agent-specific context file**: Updated with IStorageBroker pattern

**Key design decisions**:
- IStorageBroker<T> generic interface with Insert<T>, Select<T>, SelectById<T>, Update<T>, Delete<T>
- StorageBroker implements IStorageBroker, receives IConfiguration, configures DbContext
- Model-specific brokers inherit/wrap generic pattern, no code duplication
- Program.cs registration: `services.AddScoped<IStorageBroker, StorageBroker>()` + individual model brokers

**Output**: Complete design artifacts ready for `/speckit.tasks` phase

---

### Phase 2: Task Generation

When ready, execute: `/speckit-tasks`

This will generate `tasks.md` with:
- Actionable implementation tasks by priority (P1: interface + StorageBroker, P2: model-specific brokers + DI + verification)
- Dependencies and task ordering
- Test coverage mapping
- Ready for implementation

---

## Plan Approval

| Item | Status | Sign-Off |
|---|---|---|
| Constitution Check | ✅ PASS | No violations |
| Technical Context | ✅ VERIFIED | All choices established, low complexity |
| Architecture Design | ✅ READY | Three-layer pattern confirmed, generic interface pattern chosen |
| Structure Plan | ✅ APPROVED | Single-project extension, minimal new files, clear organization |
| Phases | ✅ OUTLINED | Phase 0 (research), Phase 1 (design), Phase 2 (tasks generation) mapped |

**Plan Status**: ✅ **READY FOR PHASE EXECUTION**
