# Implementation Tasks: Generic Storage Broker Integration

**Feature**: Generic Storage Broker Integration (`003-storage-broker-integration`)  
**Date**: April 17, 2026  
**Status**: Ready for Implementation  
**Scope**: StorageBroker (generic CRUD), IProfileBroker (domain interface), ProfileBroker (implementation), DI setup  
**Constraint**: Each task ≤ 30 lines (standalone commits)

---

## Overview

| Phase | Description | Tasks | Story |
|-------|-------------|-------|-------|
| Phase 1 | Setup & Foundational | T001-T004 | Setup |
| Phase 2 | StorageBroker Generic CRUD | T005-T009 | US1-US2 |
| Phase 3 | IProfileBroker Domain Interface | T010-T013 | US1-US2 |
| Phase 4 | ProfileBroker Implementation | T014-T027 | US3 |
| Phase 5 | Dependency Injection Registration | T028-T029 | US4 |
| Phase 6 | Predefined Roles & Testing | T030-T034 | US5 |

**Total Tasks**: 34 | **Parallelizable**: 18 [P] | **MVP Scope**: Phase 1-3

---

## Phase 1: Setup & Foundational

### Goal
Establish project structure and verify prerequisites for broker implementation.

### Independent Test Criteria
- ✅ Brokers/ directory created
- ✅ Required NuGet packages verified in .csproj
- ✅ No compilation errors from new empty files

---

- [ ] T001 Create Brokers directory structure in Markwell.Core/Brokers/

- [ ] T002 [P] Verify EntityFramework Core 10.0 package in .csproj for DbContext support

- [ ] T003 [P] Verify Moq package in Tests/Unit/ for broker mocking

- [ ] T004 Verify ApplicationDbContext exists in Markwell.Core/Data/ with OnConfiguring override

---

## Phase 2: StorageBroker Generic CRUD Implementation

### Goal
Implement generic CRUD broker without interface. Keep constructor minimal (DbContext only).

### Independent Test Criteria
- ✅ StorageBroker.cs file created
- ✅ All 5 CRUD methods present with correct signatures
- ✅ Generic <T> constraints applied (where T : class)
- ✅ Can instantiate with ApplicationDbContext
- ✅ Methods throw/return per contract (DbUpdateException, null for not-found)

---

- [ ] T005 Create StorageBroker.cs file in Markwell.Core/Brokers/StorageBroker.cs with class declaration and constructor

- [ ] T006 [P] Implement StorageBroker.InsertAsync<T>(T entity) method with DbContext.Add and SaveChangesAsync

- [ ] T007 [P] Implement StorageBroker.Select<T>() method returning IQueryable for LINQ support

- [ ] T008 [P] Implement StorageBroker.SelectByIdAsync<T>(string id) method with FindAsync and null return

- [ ] T009 [P] Implement StorageBroker.UpdateAsync<T>(T entity) and DeleteAsync<T>(string id) methods with exception handling

---

## Phase 3: IProfileBroker Domain Interface

### Goal
Define domain-specific interface for user, role, and profile operations. 19 methods total.

### Independent Test Criteria
- ✅ IProfileBroker.cs file created
- ✅ All 19 method signatures present
- ✅ XML documentation on all methods
- ✅ Organized into 3 sections: User (7), Role (6), Profile (6)
- ✅ Return types follow contract (nullable for reads, exceptions for writes)

---

- [ ] T010 Create IProfileBroker.cs interface file in Markwell.Core/Brokers/IProfileBroker.cs with namespace and XML header

- [ ] T011 [P] Add User CRUD methods to IProfileBroker: CreateUserAsync, GetUserByIdAsync, GetUserByEmailAsync, GetUserByUserNameAsync, GetAllUsersAsync, UpdateUserAsync, DeleteUserAsync

- [ ] T012 [P] Add Role CRUD methods to IProfileBroker: CreateRoleAsync, GetRoleByIdAsync, GetRoleByNameAsync, GetPredefinedRolesAsync, UpdateRoleAsync, DeleteRoleAsync

- [ ] T013 [P] Add Profile & Assignment methods to IProfileBroker: GetUserWithRolesAsync, GetUserRolesAsync, GetRoleUsersAsync, GetUserRoleAsync, AssignRoleToUserAsync, RemoveRoleFromUserAsync

---

## Phase 4: ProfileBroker Implementation

### Goal
Implement single ProfileBroker class wrapping StorageBroker. Keep each section focused (5-7 methods per task).

### Independent Test Criteria
- ✅ ProfileBroker.cs file created
- ✅ Implements IProfileBroker interface
- ✅ Constructor takes StorageBroker
- ✅ All 19 methods implemented
- ✅ Read methods return nullable/empty (no exceptions)
- ✅ Write methods delegate to StorageBroker and handle exceptions

---

- [ ] T014 Create ProfileBroker.cs file in Markwell.Core/Brokers/ProfileBroker.cs with class declaration and constructor

- [ ] T015 [P] Implement User CRUD methods in ProfileBroker: CreateUserAsync, GetUserByIdAsync, GetUserByEmailAsync (delegates to StorageBroker)

- [ ] T016 [P] Implement remaining User methods in ProfileBroker: GetUserByUserNameAsync, GetAllUsersAsync, UpdateUserAsync, DeleteUserAsync

- [ ] T017 [P] Implement Role CRUD methods in ProfileBroker: CreateRoleAsync, GetRoleByIdAsync, GetRoleByNameAsync (delegates to StorageBroker)

- [ ] T018 [P] Implement remaining Role methods in ProfileBroker: GetPredefinedRolesAsync, UpdateRoleAsync, DeleteRoleAsync

- [ ] T019 [P] Implement Profile queries in ProfileBroker: GetUserWithRolesAsync (Include), GetUserRolesAsync, GetRoleUsersAsync

- [ ] T020 [P] Implement Role assignment methods in ProfileBroker: GetUserRoleAsync, AssignRoleToUserAsync, RemoveRoleFromUserAsync

---

## Phase 5: Dependency Injection Registration

### Goal
Register StorageBroker and ProfileBroker in Program.cs. Keep registration minimal (3 lines).

### Independent Test Criteria
- ✅ Program.cs updated with scoped DI registration
- ✅ Only service registration (no configuration logic)
- ✅ StorageBroker registered first, then IProfileBroker → ProfileBroker
- ✅ Application builds without errors
- ✅ Services can inject IProfileBroker

---

- [ ] T021 Register StorageBroker in Program.cs with services.AddScoped<StorageBroker>()

- [ ] T022 Register IProfileBroker mapping in Program.cs with services.AddScoped<IProfileBroker, ProfileBroker>()

- [ ] T023 Verify DI registration: Create a test service that injects IProfileBroker and verify it resolves correctly

---

## Phase 6: Predefined Roles Support & Polish

### Goal
Seed predefined roles (Admin, Manager, Teacher, Student) and verify retrieval via ProfileBroker.

### Independent Test Criteria
- ✅ Predefined roles seeded in ApplicationDbContext initialization or startup
- ✅ RoleBroker.GetPredefinedRolesAsync returns all 4 roles
- ✅ Roles can be queried by ID or name via ProfileBroker methods
- ✅ All unit tests pass (StorageBroker, ProfileBroker)
- ✅ Integration tests verify end-to-end CRUD flow

---

- [ ] T024 Create RoleSeeder helper in Markwell.Core/Data/RoleSeeder.cs to create predefined roles (Admin, Manager, Teacher, Student)

- [ ] T025 [P] Create unit test file Tests/Unit/Brokers/StorageBrokerTests.cs with mocked DbContext

- [ ] T026 [P] Create unit test file Tests/Unit/Brokers/ProfileBrokerTests.cs mocking StorageBroker

- [ ] T027 Add integration test for predefined roles in Tests/Integration/Brokers/ProfileBrokerIntegrationTests.cs

- [ ] T028 [P] Add unit tests for StorageBroker CRUD methods: InsertAsync, SelectAsync, SelectByIdAsync, UpdateAsync, DeleteAsync

- [ ] T029 [P] Add unit tests for ProfileBroker user/role methods: CreateUserAsync, GetRoleByNameAsync, AssignRoleToUserAsync

- [ ] T030 Add integration test for complete profile flow: create user, assign role, retrieve with roles

- [ ] T031 Create and run RoleSeeder on application startup in Program.cs

- [ ] T032 Verify all user stories pass acceptance criteria (manual checklist)

- [ ] T033 Document broker usage in README.md with examples

- [ ] T034 Final review and code cleanup (no dead code, all tests passing)

---

## Dependency Graph & Execution Order

### Strict Dependencies

```
Phase 1 (Setup)
    ↓
Phase 2 (StorageBroker)
    ↓
Phase 3 (IProfileBroker interface)
    ↓
Phase 4 (ProfileBroker implementation) → Phase 5 (DI)
    ↓
Phase 6 (Tests & Polish)
```

### Within Each Phase: Parallelizable Tasks

**Phase 2**: T006, T007, T008, T009 (all independent CRUD implementations) - **Can run in parallel**

**Phase 3**: T011, T012, T013 (all independent method signatures) - **Can run in parallel**

**Phase 4**: T015-T020 (ProfileBroker methods by domain) - **Can run in parallel**

**Phase 6**: T025, T026, T028, T029 (unit tests) - **Can run in parallel**

---

## Execution Plan: Recommended Sequence

### Iteration 1: Foundation (1-2 hours)
1. T001, T002, T003, T004 (Setup)
2. T005-T009 (StorageBroker in parallel)
3. T010-T013 (IProfileBroker in parallel)

**Checkpoint**: Both broker files created, interfaces defined, signatures correct

### Iteration 2: ProfileBroker Implementation (2-3 hours)
1. T014 (Create ProfileBroker.cs)
2. T015-T020 in parallel (Implement all 19 methods)

**Checkpoint**: ProfileBroker fully implements IProfileBroker

### Iteration 3: Integration (1-2 hours)
1. T021-T023 (DI Registration)
2. T024 (RoleSeeder)
3. T025-T031 in parallel (Tests & Startup)

**Checkpoint**: Application starts, DI resolves IProfileBroker, roles seeded

### Iteration 4: Validation (30 min)
1. T032-T034 (User story acceptance, documentation, cleanup)

**Checkpoint**: All user stories pass, documentation complete, ready for PR

---

## MVP Scope

**Minimum Viable Product (T001-T023)**:
- ✅ StorageBroker generic CRUD (5 methods)
- ✅ IProfileBroker domain interface (19 methods)
- ✅ ProfileBroker implementation (all 19 methods)
- ✅ DI registration in Program.cs
- **Covers**: US1 (IStorageBroker pattern via StorageBroker), US2 (StorageBroker impl), US3 (ProfileBroker), US4 (DI)

**Extended Scope (T024-T034)**:
- ✅ Predefined roles support (T024, T031)
- ✅ Unit & integration tests (T025-T030)
- ✅ Documentation & polish (T032-T034)
- **Covers**: US5 (predefined roles), comprehensive testing

---

## Story Completion Matrix

| Story | Title | Primary Tasks | Dependencies | Status |
|-------|-------|---------------|--------------|--------|
| US1 | IStorageBroker Pattern | T005-T009 | T001-T004 | ✅ Implied via StorageBroker |
| US2 | StorageBroker Implementation | T005-T009 | T001-T004 | ✅ Core implementation |
| US3 | ProfileBroker Implementation | T010-T020 | T005-T013 | ✅ Single broker per user feedback |
| US4 | DI Setup in Program.cs | T021-T023 | T005-T020 | ✅ Registration only |
| US5 | Predefined Roles Support | T024, T031 | T005-T023 | ✅ Seeding & queries |

---

## Task Tracking Format

Each task follows format: `- [ ] [TaskID] [P?] [Story?] Description with file path`

- **[TaskID]**: T001-T034 in execution order
- **[P]**: Parallelizable (can run independently)
- **[Story?]**: Only on Phase 3-6 tasks mapping to user stories
- **Description**: Clear action with exact file path for implementation

---

## Quality Checklist (Before PR)

- [ ] All 34 tasks completed ✅
- [ ] Each task ≤ 30 lines (standalone commits) ✅
- [ ] StorageBroker: 5 CRUD methods, generic <T>, exception handling ✅
- [ ] IProfileBroker: 19 methods, XML docs, organized by domain ✅
- [ ] ProfileBroker: Wraps StorageBroker, implements all 19 methods ✅
- [ ] DI: Program.cs registration only (3 lines) ✅
- [ ] Tests: Unit + Integration covering all brokers ✅
- [ ] Predefined roles: Admin, Manager, Teacher, Student seeded ✅
- [ ] Documentation: README.md updated with usage examples ✅
- [ ] Build: 0 errors, 0 warnings ✅
- [ ] Git: All commits follow naming convention ✅

---

## Notes

**Estimated Time**: 6-8 hours total (1-2 hours setup, 2-3 hours implementation, 1-2 hours testing, 30 min polish)

**Risk Factors**: None - straightforward CRUD pattern, all dependencies pre-existing (DbContext, DI container)

**Post-Implementation**: Ready to proceed with US2 Phase 5-9 (tests) and other features using IProfileBroker

**Status**: ✅ **READY FOR IMPLEMENTATION**

