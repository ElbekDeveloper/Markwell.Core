# Implementation Plan: Profile Management & Role-Based Access Control

**Branch**: `002-profile-management` | **Date**: April 16, 2026 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-profile-management/spec.md`

## Summary

Central user profile management system for Markwell educational platform using ASP.NET Core 10.0 with EF Core Identity. Provides role-based access control (RBAC) with four predefined roles (Admin, Manager, Teacher, Student), user authentication via standard REST endpoints (`POST /register`, `POST /login`), and profile management. Features include user creation with role assignment, self-registration, profile updates, password management, and administrative user search/filtering. All endpoints require comprehensive test coverage via acceptance tests (.http files) and unit tests.

## Technical Context

**Language/Version**: C# 13, .NET 10.0 LTS  
**Primary Dependencies**: ASP.NET Core 10.0, EF Core 10.0, ASP.NET Core Identity, Scalar.AspNetCore 2.14.0  
**Storage**: EF Core with PostgreSQL (dev + production) and SQLite (local + testing)  
**Testing**: xUnit for unit tests, Moq for mocking, SQLite in-memory for integration test databases  
**Target Platform**: .NET 10.0 LTS on Linux or Windows servers  
**Project Type**: Web API service  
**Performance Goals**: 
- User authentication: <500ms (SC-001)
- Profile updates: <1 second (SC-002)
- Role-based authorization: <100ms (SC-003)
- Support 1000 concurrent authenticated users (SC-004)

**Constraints**: 
- Password changes effective immediately (SC-005)
- Role modifications propagate within 2 seconds (SC-006)
- Email verification tokens expire after 24 hours (SC-007)

**Scale/Scope**: 
- Educational platform with 4 role types (Admin, Manager, Teacher, Student)
- Multi-tenant awareness not required (single organization)
- Single-sign-on (SSO) out of scope

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

✅ **PASS**: All constitution principles verified:

1. **Naming Conventions**: Services will use singular naming (StudentService, UserService, RoleService). Controllers will be plural (StudentsController, UsersController). Brokers will be singular (StudentBroker, UserBroker, IdentityBroker). Methods will contain verbs; async methods postfixed with Async.

2. **Layered Architecture (Broker → Service → Controller)**: Implementation will strictly follow three-layer pattern:
   - **Brokers**: IdentityBroker (EF Core Identity interface), UserBroker (user storage), RoleBroker (role management)
   - **Services**: UserService (business logic), AuthenticationService (login/registration), RoleService (role management)
   - **Controllers**: UsersController, AuthController for REST endpoints

3. **Method Design**: Multi-line methods will have blank line before return. Parameter declarations over 120 chars placed one per line. Chaining will follow uglification pattern.

4. **Code Clarity**: Comments only for invisible logic. XML documentation required for complex methods. `var` used when type is clear; explicit types for inferred returns. Named parameters for non-variable literals.

5. **Testing Discipline** ✅ **VERIFIED**: 
   - Test-First approach: Unit tests before implementation
   - Arrange/Act/Assert structure required
   - Test names describe scenario (e.g., `ShouldThrowValidationExceptionOnRegisterWhenEmailInvalid`)
   - Integration tests required for broker contracts (Identity framework integration)
   - Mocks permitted only at broker boundary

6. **Development Workflow** ✅ **VERIFIED**:
   - No direct commits to master
   - Feature branch: `002-profile-management`
   - PR required before merge with review
   - Spec-kit workflow followed: specify → plan → tasks → implement
   - CodeRabbit validation on PR

**Status**: ✅ GATE PASSED - No conflicts with constitution. Proceed to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/002-profile-management/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   ├── auth-endpoints.contract.md
│   ├── user-profile.contract.md
│   └── role-management.contract.md
└── checklists/
    └── requirements.md  # Specification quality checklist
```

### Source Code (repository root)

```text
# Single Project: ASP.NET Core Web API (established from US1)
Markwell.Core/
├── Program.cs                    # Existing: minimal API setup, Scalar UI
├── Markwell.Core.csproj         # EF Core Identity packages added
├── Entities/
│   ├── User.cs                  # User entity extending Identity.User
│   ├── Role.cs                  # Role entity extending Identity.Role
│   └── UserRole.cs              # Join table for User-Role mapping
├── Models/
│   ├── CreateUserRequest.cs
│   ├── RegisterRequest.cs
│   ├── LoginRequest.cs
│   ├── UpdateProfileRequest.cs
│   ├── ChangePasswordRequest.cs
│   ├── UserResponse.cs
│   └── RoleAssignmentRequest.cs
├── Services/
│   ├── UserService.cs           # User management business logic
│   ├── AuthenticationService.cs # Login/registration logic
│   ├── RoleService.cs           # Role management logic
│   ├── PasswordValidationService.cs
│   └── EmailVerificationService.cs
├── Brokers/
│   ├── IdentityBroker.cs        # EF Core Identity interface
│   ├── UserBroker.cs            # User data access
│   └── RoleBroker.cs            # Role data access
├── Controllers/
│   ├── UsersController.cs       # User CRUD endpoints
│   ├── AuthController.cs        # /register, /login endpoints
│   └── RolesController.cs       # Role management endpoints
├── Data/
│   └── ApplicationDbContext.cs  # EF Core DbContext
├── Configurations/
│   ├── IdentityConfiguration.cs # EF Core Identity setup
│   └── RoleConfiguration.cs     # Default roles seeding
├── Migrations/
│   └── [Auto-generated EF Core migrations]
├── Markwell.Core.http           # Existing: Scalar UI test file
├── appsettings.json             # Existing: configuration
└── appsettings.Development.json # Existing: dev configuration

# Tests: Unit and Integration
Tests/
├── Markwell.Core.Tests.csproj
├── Unit/
│   ├── Services/
│   │   ├── UserServiceTests.cs
│   │   ├── AuthenticationServiceTests.cs
│   │   ├── RoleServiceTests.cs
│   │   └── PasswordValidationServiceTests.cs
│   └── Controllers/
│       ├── UsersControllerTests.cs
│       ├── AuthControllerTests.cs
│       └── RolesControllerTests.cs
├── Integration/
│   ├── AuthEndpointsTests.cs     # /register, /login integration
│   ├── UserProfileEndpointsTests.cs
│   ├── RoleManagementEndpointsTests.cs
│   └── DbContextTests.cs         # EF Core Identity integration
└── Fixtures/
    ├── DatabaseFixture.cs       # Test database setup
    └── UserDataBuilder.cs       # Test user creation helper
```

**Structure Decision**: Single project structure (ASP.NET Core Web API) established in US1 is extended with layered architecture: Controllers (REST endpoints) → Services (business logic) → Brokers (EF Core Identity data access). Test projects separate into Unit and Integration folders. No separation into multiple projects needed at this stage; single DbContext manages all user/role data.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | No violations | Constitution fully compliant |

---

## Phase Execution Summary

### ✅ Phase 0: Research & Clarification - COMPLETE

**Deliverable**: `research.md`

**Outcomes**:
- ✅ Database choice resolved: SQL Server (dev/LocalDB) + PostgreSQL (prod) + SQLite (test)
- ✅ ASP.NET Core Identity integration architecture designed
- ✅ Authentication endpoints confirmed: REST convention (`POST /register`, `POST /login`)
- ✅ Role management strategy finalized: flat 4 predefined roles
- ✅ Test strategy defined: unit + integration + acceptance
- ✅ Password security approach: PBKDF2-SHA256 via PasswordHasher
- ✅ Email verification workflow designed: token-based, 24-hour expiration

---

### ✅ Phase 1: Design & Contracts - COMPLETE

**Deliverables**:
- ✅ `data-model.md` - Entity relationships, attributes, constraints, indexes
- ✅ `contracts/auth-endpoints.contract.md` - POST /register, POST /login, POST /auth/confirm-email
- ✅ `contracts/user-profile.contract.md` - GET/PUT /users/{id}, POST /users/{id}/change-password, GET /users
- ✅ `contracts/role-management.contract.md` - POST/DELETE /users/{id}/roles, GET /roles
- ✅ `quickstart.md` - Feature overview, architecture, workflows, setup & testing guide

**Outcomes**:
- ✅ Complete data model with User, Role, UserRole entities
- ✅ API contracts for 10 endpoints with request/response examples and error codes
- ✅ RBAC matrix showing role-based access control rules
- ✅ Security considerations documented
- ✅ Quick start guide for developers
- ✅ Project structure finalized

---

## Next Steps: Phase 2 (Task Generation)

**When ready, execute**: `/speckit-tasks`

This will generate `tasks.md` with:
- Actionable task breakdown by phase (setup, core features, testing, verification)
- Dependencies and task ordering
- Estimated complexity and effort
- Test coverage mapping
- Ready for implementation

---

## Plan Approval

| Item | Status | Sign-Off |
|------|--------|----------|
| Constitution Check | ✅ PASS | No violations |
| Architecture Design | ✅ APPROVED | Aligns with Markwell conventions |
| Data Model | ✅ COMPLETE | 3 entities, relationships, constraints defined |
| API Contracts | ✅ COMPLETE | 10 endpoints, 3 contract files |
| Test Strategy | ✅ DEFINED | Unit, integration, acceptance approach confirmed |
| Research Artifacts | ✅ COMPLETE | All NEEDS CLARIFICATION resolved |

**Plan Status**: ✅ **READY FOR TASK GENERATION**
