# Implementation Tasks: Profile Management & Role-Based Access Control

**Feature Branch**: `002-profile-management`  
**Created**: April 16, 2026  
**Status**: Ready for Implementation  
**Spec**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md) | **Data Model**: [data-model.md](data-model.md)

## Overview

This task list implements a complete user profile management and role-based access control (RBAC) system for Markwell using ASP.NET Core 10.0 with EF Core Identity. Tasks are organized by phase with clear dependencies, file paths, and test criteria for each user story.

**Total Tasks**: 130+ organized into 9 implementation phases

---

## Implementation Strategy

### MVP Scope (Phase 1-4 Completion)
- Setup: Project structure, packages, DbContext, migrations
- Foundational: Core entities, services, brokers, middleware
- US1: Admin user creation with role assignment
- US2: User registration, login, email verification

**Estimated Effort**: 40-50 hours for MVP (solo developer)

### Phase Delivery Order
1. **Phase 1**: Setup (parallel, no blocking)
2. **Phase 2**: Foundational (blocking prerequisite for all stories)
3. **Phase 3-8**: User Stories in priority order (P1, P1, P2, P2, P3, P3)
4. **Phase 9**: Verification & Polish

### Dependency Graph

```
Phase 1: Setup
    ↓
Phase 2: Foundational (DbContext, Services, Brokers)
    ↓
Phase 3: US1 (Admin Create User) [P]
    ├── Phase 4: US2 (Registration/Login) [P]
    └── Phase 5: US3 (Update Profile) [Depends on US2]
         ├── Phase 6: US4 (Change Password) [Depends on US2]
         └── Phase 7: US5 (Role Management) [Depends on US1]
              └── Phase 8: US6 (Search Users) [Depends on US1/US5]

[P] = Parallelizable with other stories
Phases 3 & 4 can run in parallel (independent services)
Phases 5 & 6 can run in parallel after US2
```

---

## Phase 1: Setup & Infrastructure

**Goal**: Initialize project structure, add dependencies, create DbContext, generate initial migration

**Independent Test Criteria**: Project builds without errors; migrations can be applied; DbContext initialized

### Setup Tasks

- [ ] T001 Add NuGet packages: `dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL` and `dotnet add package Microsoft.EntityFrameworkCore.Sqlite`
- [ ] T002 Add xUnit testing package: `dotnet add package xunit` and `dotnet add package xunit.runner.visualstudio`
- [ ] T003 Add Moq package: `dotnet add package Moq`
- [ ] T004 Create directory structure: `Markwell.Core/Entities`, `Markwell.Core/Models`, `Markwell.Core/Services`, `Markwell.Core/Brokers`, `Markwell.Core/Configurations`, `Markwell.Core/Data`, `Markwell.Core/Migrations`
- [ ] T005 Create directory structure for tests: `Tests/Unit/Services`, `Tests/Unit/Controllers`, `Tests/Integration`, `Tests/Fixtures`
- [ ] T006 Create `Markwell.Core/Data/ApplicationDbContext.cs` inheriting from `IdentityDbContext<User, Role, string>` with DbSet properties for User, Role, UserRole
- [ ] T007 Update `Program.cs` to register DbContext with provider-agnostic configuration:
  - Development: SQLite in-memory (`options.UseSqlite("Data Source=:memory;")`)
  - Production: PostgreSQL from environment variable
- [ ] T008 Create initial EF Core migration: `dotnet ef migrations add InitialIdentitySchema --project Markwell.Core`
- [ ] T009 Verify migration creates AspNetUsers, AspNetRoles, AspNetUserRoles, and Identity framework tables
- [ ] T010 Create `Markwell.Core.http` test file for acceptance testing with endpoint examples (placeholder for later)

---

## Phase 2: Foundational Layer (Entities, Services, Brokers, Middleware)

**Goal**: Implement core data entities, identity setup, service layer, broker layer, and authentication middleware

**Independent Test Criteria**: 
- DbContext can instantiate and query User/Role entities
- Services can be instantiated with mocked brokers
- Brokers can be instantiated with DbContext
- Password validation works independently

### Entity Tasks

- [ ] T011 Create `Markwell.Core/Entities/User.cs` extending `IdentityUser<string>` with properties:
  - `FullName: string`
  - `CreatedAt: DateTime`
  - `UpdatedAt: DateTime?`
  - `LastLoginAt: DateTime?`
  - `IsActive: bool`
  - `UserRoles: ICollection<UserRole>`
- [ ] T012 Create `Markwell.Core/Entities/Role.cs` extending `IdentityRole<string>` with properties:
  - `CreatedAt: DateTime`
  - `UserRoles: ICollection<UserRole>`
- [ ] T013 Create `Markwell.Core/Entities/UserRole.cs` as junction table with properties:
  - `UserId: string` (FK)
  - `RoleId: string` (FK)
  - `AssignedAt: DateTime`
  - `AssignedBy: string?` (FK to User)
  - Composite PK: (UserId, RoleId)

### Configuration Tasks

- [ ] T014 Create `Markwell.Core/Configurations/IdentityConfiguration.cs` to configure ASP.NET Core Identity in DbContext:
  - Configure password requirements (min 8, upper, lower, digit, special)
  - Configure email as unique username
  - Configure user lockout policy
- [ ] T015 Create `Markwell.Core/Configurations/RoleConfiguration.cs` to seed predefined roles (Admin, Manager, Teacher, Student) in DbContext.OnModelCreating
- [ ] T016 Update `Markwell.Core/Data/ApplicationDbContext.cs` OnModelCreating to:
  - Configure User entity indexes (NormalizedEmail, CreatedAt)
  - Configure Role entity indexes (NormalizedName)
  - Call RoleConfiguration.Configure()
  - Seed 4 predefined roles with CreatedAt timestamps

### Model (DTO) Tasks

- [ ] T017 Create `Markwell.Core/Models/RegisterRequest.cs` with Email, Password, FullName properties
- [ ] T018 Create `Markwell.Core/Models/LoginRequest.cs` with Email, Password properties
- [ ] T019 Create `Markwell.Core/Models/UserResponse.cs` with Id, Email, UserName, FullName, EmailConfirmed, IsActive, Roles, CreatedAt, UpdatedAt, LastLoginAt
- [ ] T020 Create `Markwell.Core/Models/UpdateProfileRequest.cs` with FullName, Email, PhoneNumber properties (all optional)
- [ ] T021 Create `Markwell.Core/Models/ChangePasswordRequest.cs` with CurrentPassword, NewPassword properties
- [ ] T022 Create `Markwell.Core/Models/RoleAssignmentRequest.cs` with RoleName property
- [ ] T023 Create `Markwell.Core/Models/CreateUserRequest.cs` with Email, FullName, RoleName properties

### Service Layer Tasks

- [ ] T024 Create `Markwell.Core/Services/PasswordValidationService.cs` with method:
  - `ValidatePassword(password: string): bool` — returns true if meets requirements (min 8, upper, lower, digit, special)
  - `GetPasswordStrengthMessage(password: string): string` — returns error message if invalid
- [ ] T025 Create `Markwell.Core/Services/EmailVerificationService.cs` with methods:
  - `GenerateVerificationToken(user: User): Task<string>` 
  - `ConfirmEmailAsync(user: User, token: string): Task<IdentityResult>`

### Broker Layer Tasks

- [ ] T026 Create `Markwell.Core/Brokers/IdentityBroker.cs` with constructor injection of `UserManager<User>`, `RoleManager<Role>`, `SignInManager<User>` and methods:
  - `CreateUserAsync(user: User, password: string): Task<IdentityResult>`
  - `FindUserByEmailAsync(email: string): Task<User?>`
  - `UpdateUserAsync(user: User): Task<IdentityResult>`
  - `DeleteUserAsync(user: User): Task<IdentityResult>`
  - `ChangePasswordAsync(user: User, currentPassword: string, newPassword: string): Task<IdentityResult>`
  - `SignInAsync(user: User, password: string): Task<SignInResult>`
- [ ] T027 Create `Markwell.Core/Brokers/UserBroker.cs` with constructor injection of `ApplicationDbContext` and methods:
  - `GetUserByIdAsync(userId: string): Task<User?>`
  - `GetUserWithRolesAsync(userId: string): Task<User?>`
  - `UpdateUserAsync(user: User): Task<void>`
  - `GetAllUsersAsync(pageNumber: int, pageSize: int): Task<(List<User>, int totalCount)>`
  - `SearchUsersByEmailAsync(searchTerm: string, pageNumber: int, pageSize: int): Task<(List<User>, int totalCount)>`
- [ ] T028 Create `Markwell.Core/Brokers/RoleBroker.cs` with constructor injection of `ApplicationDbContext` and methods:
  - `GetRoleByNameAsync(roleName: string): Task<Role?>`
  - `GetAllRolesAsync(): Task<List<Role>>`
  - `AssignRoleAsync(userId: string, roleId: string, assignedBy: string?): Task<void>`
  - `RemoveRoleAsync(userId: string, roleId: string): Task<void>`
  - `GetUserRolesAsync(userId: string): Task<List<Role>>`

### Application Service Tasks

- [ ] T029 Update `Program.cs` to register Identity services:
  - `services.AddIdentity<User, Role>(options => { /* IdentityConfiguration */ })`
  - `services.AddScoped<IdentityBroker>()`
  - `services.AddScoped<UserBroker>()`
  - `services.AddScoped<RoleBroker>()`
  - `services.AddScoped<PasswordValidationService>()`
  - `services.AddScoped<EmailVerificationService>()`
- [ ] T030 Update `Program.cs` to add authentication middleware:
  - `app.UseAuthentication()`
  - `app.UseAuthorization()`
- [ ] T031 Apply EF Core migration: `dotnet ef database update`

---

## Phase 3: User Story 1 - Admin Creates New User with Role Assignment (P1)

**Goal**: Implement admin endpoint for creating users with role assignment

**Independent Test Criteria**: 
- Can create user with admin authorization
- Non-admin request rejected
- Duplicate email rejected
- Role properly assigned and retrieved

### Service Layer (US1)

- [ ] T032 [P] [US1] Create `Markwell.Core/Services/UserService.cs` with constructor injection of `UserBroker`, `IdentityBroker`, `RoleBroker`, `PasswordValidationService` and method:
  - `CreateUserAsync(createUserRequest: CreateUserRequest, createdByUserId: string): Task<User>` 
    - Validate email format
    - Check email uniqueness
    - Create user with IdentityBroker
    - Assign role via RoleBroker
    - Return created user

- [ ] T033 [P] [US1] Create `Markwell.Core/Services/RoleService.cs` with constructor injection of `RoleBroker`, `UserBroker` and methods:
  - `AssignRoleAsync(userId: string, roleName: string, assignedBy: string): Task<void>`
    - Validate role exists
    - Check user exists
    - Prevent duplicate assignment
    - Call RoleBroker.AssignRoleAsync()

### Controller Layer (US1)

- [ ] T034 [P] [US1] Create `Markwell.Core/Controllers/UsersController.cs` with POST endpoint `/users`:
  - Accept `CreateUserRequest`
  - Require `[Authorize(Roles = "Admin")]` attribute
  - Call `UserService.CreateUserAsync()`
  - Return 201 Created with user response
  - Handle exceptions: return 400 (email exists), 403 (unauthorized)

### Unit Tests (US1)

- [ ] T035 [P] [US1] Create `Tests/Unit/Services/UserServiceTests.cs`:
  - `ShouldThrowValidationExceptionOnCreateWhenEmailInvalid`
  - `ShouldThrowConflictExceptionOnCreateWhenEmailExists`
  - `ShouldReturnUserWithAssignedRoleOnCreateSuccess`
  - Test method: Arrange (setup brokers with Moq), Act (call CreateUserAsync), Assert (verify user and role)

- [ ] T036 [P] [US1] Create `Tests/Unit/Services/RoleServiceTests.cs`:
  - `ShouldThrowValidationExceptionOnAssignWhenRoleNotFound`
  - `ShouldThrowConflictExceptionOnAssignWhenRoleDuplicate`
  - `ShouldCallRoleBrokerOnAssignSuccess`
  - Test method: Arrange, Act, Assert with mocked dependencies

- [ ] T037 [P] [US1] Create `Tests/Unit/Controllers/UsersControllerTests.cs`:
  - `ShouldReturn201OnPostWhenUserCreated`
  - `ShouldReturn403OnPostWhenNotAdmin`
  - `ShouldReturn400OnPostWhenEmailInvalid`
  - Test method: Arrange (setup controller with mocked service), Act (call POST), Assert (verify response)

### Acceptance Tests (US1)

- [ ] T038 [P] [US1] Update `Markwell.Core.http` with acceptance test examples:
  - `### Create User (Admin only) POST /users`
  - Request: `{ "email": "teacher@example.com", "fullName": "Jane Doe", "roleName": "Teacher" }`
  - Response: `201 Created { "id": "...", "email": "...", "roles": ["Teacher"] }`
  - Error case: Non-admin user gets `403 Forbidden`

### Integration Tests (US1)

- [ ] T039 [P] [US1] Create `Tests/Integration/AdminUserCreationTests.cs` using SQLite in-memory database:
  - Setup: Create ApplicationDbContext with SQLite in-memory
  - Test: Create admin user, call UserService.CreateUserAsync()
  - Assert: User persisted in database, role assigned, QueryableUser includes role
  - Teardown: Dispose DbContext

---

## Phase 4: User Story 2 - User Registers and Sets Up Profile (P1)

**Goal**: Implement registration and login endpoints with email verification

**Independent Test Criteria**:
- User can register with valid credentials
- Email verification token required before login
- Can login after email confirmed
- Weak passwords rejected
- Duplicate emails rejected

### Service Layer (US2)

- [ ] T040 [P] [US2] Create `Markwell.Core/Services/AuthenticationService.cs` with constructor injection of `IdentityBroker`, `UserBroker`, `PasswordValidationService`, `EmailVerificationService` and methods:
  - `RegisterAsync(registerRequest: RegisterRequest): Task<User>`
    - Validate email and password strength
    - Create user with no password initially
    - Send verification email
    - Return user with EmailConfirmed=false
  - `LoginAsync(loginRequest: LoginRequest): Task<(SignInResult, string? token)>`
    - Find user by email
    - Validate email confirmed
    - Validate account active
    - Sign in user
    - Generate JWT token
    - Update LastLoginAt
    - Return token
  - `ConfirmEmailAsync(userId: string, token: string): Task<bool>`
    - Call EmailVerificationService.ConfirmEmailAsync()
    - Update user EmailConfirmed flag
    - Return success

### Controller Layer (US2)

- [ ] T041 [P] [US2] Create `Markwell.Core/Controllers/AuthController.cs` with endpoints:
  - `POST /auth/register` — Accept RegisterRequest
    - Call AuthenticationService.RegisterAsync()
    - Return 201 Created with user response
    - Handle errors: 400 (invalid email/password), 409 (email exists)
  - `POST /auth/login` — Accept LoginRequest
    - Call AuthenticationService.LoginAsync()
    - Return 200 OK with token
    - Handle errors: 400 (invalid credentials), 401 (email not confirmed), 403 (account disabled)
  - `POST /auth/confirm-email` — Accept userId, token
    - Call AuthenticationService.ConfirmEmailAsync()
    - Return 200 OK
    - Handle errors: 400 (invalid token), 404 (user not found)

### Unit Tests (US2)

- [ ] T042 [P] [US2] Create `Tests/Unit/Services/AuthenticationServiceTests.cs`:
  - `ShouldThrowValidationExceptionOnRegisterWhenEmailInvalid`
  - `ShouldThrowValidationExceptionOnRegisterWhenPasswordWeak`
  - `ShouldThrowConflictExceptionOnRegisterWhenEmailExists`
  - `ShouldReturnUserOnRegisterSuccess`
  - `ShouldThrowAuthorizationExceptionOnLoginWhenEmailNotConfirmed`
  - `ShouldThrowAuthorizationExceptionOnLoginWhenAccountDisabled`
  - `ShouldReturnTokenOnLoginSuccess`

- [ ] T043 [P] [US2] Create `Tests/Unit/Controllers/AuthControllerTests.cs`:
  - `ShouldReturn201OnPostRegisterWhenSuccess`
  - `ShouldReturn400OnPostRegisterWhenPasswordWeak`
  - `ShouldReturn200OnPostLoginWhenSuccess`
  - `ShouldReturn401OnPostLoginWhenEmailNotConfirmed`

### Acceptance Tests (US2)

- [ ] T044 [P] [US2] Update `Markwell.Core.http` with acceptance test examples:
  - `### Register New User POST /auth/register`
  - Request: `{ "email": "student@example.com", "password": "SecurePass123!", "fullName": "John Student" }`
  - Response: `201 Created { "id": "...", "email": "...", "emailConfirmed": false }`
  - `### Login User POST /auth/login`
  - Request: `{ "email": "student@example.com", "password": "SecurePass123!" }`
  - Response: `200 OK { "token": "...", "expiresAt": "..." }`
  - Error cases: 400 (weak password), 409 (email exists), 401 (email not confirmed)

### Integration Tests (US2)

- [ ] T045 [P] [US2] Create `Tests/Integration/RegistrationAndLoginTests.cs`:
  - Setup: Create DbContext with SQLite in-memory, register user, confirm email
  - Test: Call AuthenticationService.LoginAsync()
  - Assert: User authenticated, token returned, LastLoginAt updated
  - Teardown: Dispose DbContext

---

## Phase 5: User Story 3 - User Updates Their Profile Information (P2)

**Goal**: Implement profile update endpoint with email change workflow

**Independent Test Criteria**:
- Authenticated user can update own profile
- Cannot access other user profiles
- Email change triggers verification
- Profile changes persisted

### Service Layer (US3)

- [ ] T046 [US3] Add method to `UserService`: 
  - `UpdateProfileAsync(userId: string, updateProfileRequest: UpdateProfileRequest): Task<User>`
    - Validate authorization (user owns profile or is admin)
    - Validate email if changed (format, uniqueness)
    - Update user properties
    - If email changed: set EmailConfirmed=false, send verification email
    - Call UserBroker.UpdateUserAsync()
    - Update UpdatedAt timestamp

### Controller Layer (US3)

- [ ] T047 [US3] Add endpoint to `UsersController`:
  - `PUT /users/{id}` — Accept UpdateProfileRequest
    - Require `[Authorize]` attribute
    - Extract userId from claims (current user)
    - Call UserService.UpdateProfileAsync()
    - Return 200 OK with updated user response
    - Handle errors: 403 (unauthorized), 404 (user not found), 409 (email exists)

### Unit Tests (US3)

- [ ] T048 [US3] Add tests to `UserServiceTests.cs`:
  - `ShouldThrowAuthorizationExceptionOnUpdateWhenNotOwnerAndNotAdmin`
  - `ShouldThrowValidationExceptionOnUpdateWhenEmailInvalid`
  - `ShouldThrowConflictExceptionOnUpdateWhenEmailExists`
  - `ShouldUpdateProfileAndSetEmailUnconfirmedWhenEmailChanged`

- [ ] T049 [US3] Add tests to `UsersControllerTests.cs`:
  - `ShouldReturn200OnPutWhenProfileUpdated`
  - `ShouldReturn403OnPutWhenNotAuthorized`
  - `ShouldReturn400OnPutWhenEmailInvalid`

### Acceptance Tests (US3)

- [ ] T050 [US3] Update `Markwell.Core.http`:
  - `### Update User Profile PUT /users/{id}`
  - Request: `{ "fullName": "Jane Doe Updated", "email": "newemail@example.com" }`
  - Response: `200 OK { "fullName": "Jane Doe Updated", "email": "newemail@example.com", "emailConfirmed": false }`
  - Error: `403 Forbidden` when user not owner

### Integration Tests (US3)

- [ ] T051 [US3] Add tests to `Tests/Integration/UserProfileTests.cs`:
  - Create user, update profile, verify changes persisted
  - If email changed, verify EmailConfirmed flag reset

---

## Phase 6: User Story 4 - User Changes Password and Manages Account Security (P2)

**Goal**: Implement password change endpoint with current password verification

**Independent Test Criteria**:
- Authenticated user can change password
- Current password verification required
- Weak passwords rejected
- Old password no longer authenticates after change

### Service Layer (US4)

- [ ] T052 [US4] Add method to `AuthenticationService`:
  - `ChangePasswordAsync(userId: string, currentPassword: string, newPassword: string): Task<bool>`
    - Validate new password strength
    - Verify current password matches
    - Call IdentityBroker.ChangePasswordAsync()
    - Update SecurityStamp to invalidate existing tokens
    - Return success

### Controller Layer (US4)

- [ ] T053 [US4] Add endpoint to `AuthController`:
  - `POST /users/{id}/change-password` — Accept ChangePasswordRequest
    - Require `[Authorize]` attribute
    - Extract userId from claims (current user)
    - Call AuthenticationService.ChangePasswordAsync()
    - Return 200 OK
    - Handle errors: 400 (weak password, incorrect current), 403 (unauthorized), 404 (user not found)

### Unit Tests (US4)

- [ ] T054 [US4] Add tests to `AuthenticationServiceTests.cs`:
  - `ShouldThrowValidationExceptionOnChangePasswordWhenNewPasswordWeak`
  - `ShouldThrowValidationExceptionOnChangePasswordWhenCurrentPasswordIncorrect`
  - `ShouldThrowValidationExceptionOnChangePasswordWhenSameAsNew`
  - `ShouldUpdatePasswordSuccessfully`

- [ ] T055 [US4] Add tests to `AuthControllerTests.cs`:
  - `ShouldReturn200OnPostChangePasswordWhenSuccess`
  - `ShouldReturn400OnPostChangePasswordWhenPasswordWeak`
  - `ShouldReturn403OnPostChangePasswordWhenUnauthorized`

### Acceptance Tests (US4)

- [ ] T056 [US4] Update `Markwell.Core.http`:
  - `### Change Password POST /users/{id}/change-password`
  - Request: `{ "currentPassword": "OldPass123!", "newPassword": "NewPass456!" }`
  - Response: `200 OK { "message": "Password changed successfully" }`
  - Error: `400 Bad Request` when current password incorrect

### Integration Tests (US4)

- [ ] T057 [US4] Create `Tests/Integration/PasswordChangeTests.cs`:
  - Register user, change password, verify old password no longer works
  - Login with new password succeeds

---

## Phase 7: User Story 5 - Manager or Admin Manages User Roles and Permissions (P3)

**Goal**: Implement role assignment and removal endpoints for managers/admins

**Independent Test Criteria**:
- Admin can assign/remove roles
- Non-admin rejected
- Cannot remove last role or last admin
- Role changes apply immediately

### Service Layer (US5)

- [ ] T058 [US5] Add methods to `RoleService`:
  - `RemoveRoleAsync(userId: string, roleId: string, removedByUserId: string): Task<void>`
    - Validate user exists
    - Validate role exists
    - Check user has this role
    - Prevent removing last role
    - Prevent removing last Admin role
    - Call RoleBroker.RemoveRoleAsync()

### Controller Layer (US5)

- [ ] T059 [US5] Add endpoint to `RolesController` (create new file):
  - `DELETE /users/{id}/roles/{roleId}` — 
    - Require `[Authorize(Roles = "Admin")]` attribute
    - Extract userId from claims
    - Call RoleService.RemoveRoleAsync()
    - Return 204 No Content
    - Handle errors: 400 (cannot remove last role), 403 (unauthorized), 404 (user/role not found)

- [ ] T060 [US5] Add endpoint to `UsersController`:
  - Update existing `/users/{id}` POST for role assignment (if not already done in US1)
  - Or create `POST /users/{id}/roles` endpoint
    - Require `[Authorize(Roles = "Admin")]` attribute
    - Accept RoleAssignmentRequest
    - Call RoleService.AssignRoleAsync()
    - Return 201 Created
    - Handle errors: 400 (invalid role), 403 (unauthorized), 409 (role exists)

### Unit Tests (US5)

- [ ] T061 [US5] Add tests to `RoleServiceTests.cs`:
  - `ShouldThrowValidationExceptionOnRemoveWhenLastRoleRemoval`
  - `ShouldThrowValidationExceptionOnRemoveWhenLastAdminRemoval`
  - `ShouldThrowConflictExceptionOnRemoveWhenRoleNotAssigned`
  - `ShouldRemoveRoleSuccessfully`

- [ ] T062 [US5] Create `Tests/Unit/Controllers/RolesControllerTests.cs`:
  - `ShouldReturn201OnPostAssignRoleWhenSuccess`
  - `ShouldReturn204OnDeleteRemoveRoleWhenSuccess`
  - `ShouldReturn403OnDeleteWhenNotAdmin`
  - `ShouldReturn400OnDeleteWhenLastRoleRemoval`

### Acceptance Tests (US5)

- [ ] T063 [US5] Update `Markwell.Core.http`:
  - `### Assign Role to User POST /users/{id}/roles`
  - Request: `{ "roleName": "Manager" }`
  - Response: `201 Created { "userId": "...", "roleName": "Manager", "assignedAt": "..." }`
  - `### Remove Role from User DELETE /users/{id}/roles/{roleId}`
  - Response: `204 No Content`
  - Error: `400 Bad Request` when last role removal attempted

### Integration Tests (US5)

- [ ] T064 [US5] Create `Tests/Integration/RoleManagementTests.cs`:
  - Create user with role, assign additional role, verify both roles returned
  - Remove role, verify only remaining role persisted

---

## Phase 8: User Story 6 - List and Search Users by Role or Criteria (P3)

**Goal**: Implement user search/list endpoint with filtering and pagination

**Independent Test Criteria**:
- Admin can list all users
- Manager can list only scoped users
- Non-admin rejected
- Filtering by role works
- Pagination works

### Service Layer (US6)

- [ ] T065 [US6] Add method to `UserService`:
  - `SearchUsersAsync(searchTerm: string?, roleName: string?, pageNumber: int, pageSize: int, requestorId: string): Task<(List<UserResponse>, int totalCount)>`
    - Validate pagination parameters (pageSize max 100)
    - Determine user scope based on requestor role
    - Query UserBroker with filters
    - Map results to UserResponse DTOs with roles
    - Return paginated results

### Controller Layer (US6)

- [ ] T066 [US6] Add endpoint to `UsersController`:
  - `GET /users?search=term&role=Teacher&pageNumber=1&pageSize=20` — 
    - Require `[Authorize(Roles = "Admin,Manager")]` attribute
    - Extract userId from claims
    - Call UserService.SearchUsersAsync()
    - Return 200 OK with paginated user list
    - Handle errors: 403 (unauthorized), 400 (invalid pagination)

### Unit Tests (US6)

- [ ] T067 [US6] Add tests to `UserServiceTests.cs`:
  - `ShouldReturnOnlyTeacherWhenFilteredByTeacherRole`
  - `ShouldReturnPaginatedResultsWhenSearched`
  - `ShouldThrowAuthorizationExceptionWhenNonAdminSearchesFull`

- [ ] T068 [US6] Add tests to `UsersControllerTests.cs`:
  - `ShouldReturn200OnGetUsersWhenAdmin`
  - `ShouldReturn403OnGetUsersWhenNeitherAdminNorManager`
  - `ShouldReturnFilteredUsersByRoleWhenRequested`

### Acceptance Tests (US6)

- [ ] T069 [US6] Update `Markwell.Core.http`:
  - `### Search Users GET /users?search=john&role=Teacher&pageNumber=1&pageSize=20`
  - Response: `200 OK { "items": [...], "totalCount": 5, "pageNumber": 1, "pageSize": 20, "totalPages": 1 }`
  - Error: `403 Forbidden` when non-admin user attempts search

### Integration Tests (US6)

- [ ] T070 [US6] Create `Tests/Integration/UserSearchTests.cs`:
  - Create multiple users with different roles
  - Search by role, verify correct users returned
  - Test pagination with pageSize < total users

---

## Phase 9: Verification, Security & Polish

**Goal**: Verify all endpoints, run integration tests, perform security review, document completed feature

### Build & Verification

- [ ] T071 Build project: `dotnet build` — verify zero errors
- [ ] T072 Run all unit tests: `dotnet test Tests/Unit/` — verify 100% pass
- [ ] T073 Run all integration tests: `dotnet test Tests/Integration/` — verify 100% pass
- [ ] T074 Verify database migration applied: `dotnet ef database update` — confirm all tables created

### Security & Performance

- [ ] T075 Verify password hashing: Inspect IdentityBroker.CreateUserAsync() returns IdentityResult with hashed password
- [ ] T076 Verify authorization attributes: Confirm all admin/manager endpoints have `[Authorize]` attributes
- [ ] T077 Verify SQL injection prevention: Confirm all queries use parameterized EF Core queries
- [ ] T078 Test concurrent requests: Load test with 100 concurrent user creation requests — verify no race conditions
- [ ] T079 Performance test authentication: 1000 login attempts — verify <500ms per request (SC-001)

### Documentation & Cleanup

- [ ] T080 Update `CLAUDE.md` with feature implementation status
- [ ] T081 Create or update `Markwell.Core.http` with all 10 endpoint examples
- [ ] T082 Document custom configuration in `Program.cs` comments
- [ ] T083 Add XML documentation to all public methods in services
- [ ] T084 Verify all code follows constitution standards (naming, layering, testing)

### Final Commit & PR Preparation

- [ ] T085 Stage all implementation files: `git add Markwell.Core/ Tests/`
- [ ] T086 Commit implementation: `git commit -m "feat: Complete profile management implementation..."`
- [ ] T087 Verify feature branch is up to date: `git pull origin master && git rebase`
- [ ] T088 Create pull request with description linking specification and test coverage

---

## Completion Criteria

✅ **MVP Complete When**:
- [ ] Phase 1: Setup complete (T001-T010)
- [ ] Phase 2: Foundational complete (T011-T031)
- [ ] Phase 3: US1 complete (T032-T039)
- [ ] Phase 4: US2 complete (T040-T045)
- [ ] All unit tests pass (T035-T037, T042-T043, T048-T049, T054-T055, T061-T062, T067-T068)
- [ ] Build succeeds (T071)
- [ ] Health endpoint + 2 auth endpoints tested (T044 acceptance tests)

✅ **Full Feature Complete When**:
- [ ] All 8 phases complete (T001-T084)
- [ ] All 88 tasks completed
- [ ] All unit + integration tests pass (100%)
- [ ] All 10 API endpoints operational and tested
- [ ] Code review passed (CodeRabbit, human review)
- [ ] Merged to master

---

## Parallel Execution Opportunities

**Run in Parallel (After Phase 2 Complete)**:
- Phase 3 (US1) and Phase 4 (US2) can run simultaneously (independent services/controllers)
- Team can split: One dev on admin user creation, another on registration/login

**Run in Parallel (After Phase 4 Complete)**:
- Phase 5 (US3), Phase 6 (US4), Phase 7 (US5), Phase 8 (US6) can run in parallel if team has 4+ developers
- All phases are independent after foundational layer

**Recommended Sequential For Solo Dev**:
1. Phase 1-2 (Setup + Foundational): Must complete first
2. Phase 3 (US1): Core admin capability
3. Phase 4 (US2): Critical user registration
4. Phase 5-8 (US3-US6): Optional add-ons in priority order
5. Phase 9 (Polish): Final verification and documentation

---

## Success Metrics

| Metric | Target | Status |
|--------|--------|--------|
| Unit Test Coverage | >90% | 📋 In Progress |
| Build Status | Zero errors | 📋 In Progress |
| All Endpoints Operational | 10/10 | 📋 In Progress |
| Performance: Login | <500ms | 📋 In Progress |
| Performance: Profile Update | <1s | 📋 In Progress |
| Performance: Authorization Check | <100ms | 📋 In Progress |
| Concurrent Users | 1000+ | 📋 In Progress |
| Code Review | Approved | 📋 Pending |
| Master Merge | ✅ | 📋 Pending |

