# Quick Start: Profile Management Feature

**Date**: April 16, 2026  
**Feature**: Profile Management & Role-Based Access Control  
**Target Audience**: Developers implementing or testing this feature

## Overview

This quick start guide provides a condensed walkthrough of the Profile Management feature architecture, key endpoints, and testing workflow.

## Feature Summary

**Goal**: Provide central user profile management and role-based access control (RBAC) for Markwell educational platform using ASP.NET Core Identity.

**Key Capabilities**:
- User registration and authentication (`POST /register`, `POST /login`)
- Email verification workflow
- Profile management (view, update, password change)
- Role management (Admin, Manager, Teacher, Student)
- User search/filtering for admins

**Tech Stack**:
- Language: C# 13
- Framework: ASP.NET Core 10.0 LTS
- ORM: Entity Framework Core 10.0
- Auth: ASP.NET Core Identity with custom roles
- Database: SQL Server (dev LocalDB) / PostgreSQL (prod) / SQLite (test)
- Testing: xUnit + Moq (unit), SQLite in-memory (integration), .http files (acceptance)

## Architecture Overview

```
┌─ REST Endpoints ────────────────────────────────────────────────┐
│  /auth/register  /auth/login  /auth/confirm-email               │
│  /users/{id}  PUT /users/{id}  /users/{id}/change-password      │
│  /users  /users/{id}/roles  DELETE /users/{id}/roles/{roleId}  │
└─────────────────────────────────────────────────────────────────┘
         │
         ▼
┌─ Controllers (REST API) ─────────────────────────────────────────┐
│  AuthController  UsersController  RolesController                │
│  - Validate input  - Parse tokens  - Return JSON responses       │
└─────────────────────────────────────────────────────────────────┘
         │
         ▼
┌─ Services (Business Logic) ──────────────────────────────────────┐
│  AuthenticationService   UserService    RoleService              │
│  - Password validation   - Profile mgmt  - Role assignment       │
│  - Token generation      - Authorization - Audit logging         │
└─────────────────────────────────────────────────────────────────┘
         │
         ▼
┌─ Brokers (Data Access) ──────────────────────────────────────────┐
│  IdentityBroker   UserBroker   RoleBroker                        │
│  - EF Core queries  - User persistence  - Role queries           │
│  - Password hashing - Database ops      - Join table ops        │
└─────────────────────────────────────────────────────────────────┘
         │
         ▼
┌─ Database ────────────────────────────────────────────────────────┐
│  AspNetUsers  AspNetRoles  AspNetUserRoles  (+ Identity tables)  │
└─────────────────────────────────────────────────────────────────┘
```

## Data Model Essentials

### Core Entities

**User** (extends IdentityUser)
- Email, UserName, PasswordHash (bcrypt)
- FullName, PhoneNumber (optional)
- EmailConfirmed, IsActive (soft delete)
- CreatedAt, UpdatedAt, LastLoginAt
- Many-to-many to Roles via UserRole

**Role** (extends IdentityRole)
- Four predefined: Admin, Manager, Teacher, Student
- CreatedAt timestamp
- Many-to-many to Users via UserRole

**UserRole** (junction table)
- Composite PK: (UserId, RoleId)
- Tracks AssignedAt, AssignedBy (audit)

### Key Constraints
- User must have email (unique, required)
- User must have at least one role
- Roles are flat (no hierarchy)
- Passwords: min 8 chars, require upper + lower + digit + special char
- Email verification tokens valid for 24 hours

## API Endpoints at a Glance

### Authentication (`/auth/...`)

| Endpoint | Method | Purpose | Auth | Response |
|----------|--------|---------|------|----------|
| `/auth/register` | POST | Create account | ❌ | 201 Created, user JSON |
| `/auth/login` | POST | Authenticate | ❌ | 200 OK, JWT token |
| `/auth/confirm-email` | POST | Verify email | ❌ | 200 OK, confirmation msg |

### User Profiles (`/users/{id}`)

| Endpoint | Method | Purpose | Auth | Role Restriction |
|----------|--------|---------|------|------------------|
| `/users/{id}` | GET | View profile | ✅ JWT | Self or Admin |
| `/users/{id}` | PUT | Update profile | ✅ JWT | Self or Admin |
| `/users/{id}/change-password` | POST | Change password | ✅ JWT | Self only |
| `/users` | GET | List/search users | ✅ JWT | Admin/Manager |

### Role Management (`/users/{id}/roles`, `/roles`)

| Endpoint | Method | Purpose | Auth | Role Restriction |
|----------|--------|---------|------|------------------|
| `/users/{id}/roles` | POST | Assign role | ✅ JWT | Admin only |
| `/users/{id}/roles/{roleId}` | DELETE | Remove role | ✅ JWT | Admin only |
| `/users/{id}/roles` | GET | List user roles | ✅ JWT | Self or Admin |
| `/roles` | GET | List all roles | ✅ JWT | All authenticated |

## Typical User Workflows

### Workflow 1: Student Self-Registration

```
1. Student visits registration page
2. POST /auth/register with email, password, fullName
   → Returns 201 Created, user.id
   → Email sent with verification link
3. Student clicks email link
   → Contains userId + token (URL params)
4. POST /auth/confirm-email with userId, token
   → Returns 200 OK, emailConfirmed=true
5. Student POST /auth/login with email, password
   → Returns 200 OK + JWT token
6. Student GET /users/{id} with JWT token
   → Returns own profile (roles: ["Student"])
```

### Workflow 2: Admin Creates Teacher Account

```
1. Admin authenticates → has JWT token with role=["Admin"]
2. Admin POST /users (custom endpoint, create user directly)
   → Creates user with email, password, roles=[Teacher]
   → Returns 201 Created, user.id
3. Admin POST /users/{id}/roles with roleName="Manager"
   → Assigns additional role
4. Teacher can now login and will have roles: ["Teacher", "Manager"]
```

### Workflow 3: User Updates Profile

```
1. User authenticates → JWT token
2. User PUT /users/{id} with fullName="Jane Doe", phoneNumber="+1234567890"
   → Returns 200 OK, updated profile
3. If email changed in same request:
   → Email changed to new value
   → EmailConfirmed set to false
   → Verification email sent
   → User must confirm before new email is active
```

## Testing Strategy

### Unit Tests (xUnit + Moq)

**Location**: `Tests/Unit/Services/`, `Tests/Unit/Controllers/`

**Examples**:
- `ShouldThrowValidationExceptionOnRegisterWhenEmailInvalid`
- `ShouldThrowValidationExceptionOnRegisterWhenPasswordWeak`
- `ShouldThrowConflictExceptionOnLoginWhenEmailNotConfirmed`
- `ShouldThrowAuthorizationExceptionOnUpdateWhenUserNotAdmin`

### Integration Tests (SQLite In-Memory)

**Location**: `Tests/Integration/`

**Examples**:
- User creation → role assignment → role query returns correct roles
- Email verification token generation → token validation
- Password hashing → bcrypt verification

### Acceptance Tests (.http Files)

**Location**: `Markwell.Core.http` (extended)

**Examples**:
```http
### Register new student
POST https://localhost:5129/auth/register
Content-Type: application/json

{
  "email": "student@example.com",
  "password": "SecurePass123!",
  "fullName": "Jane Doe"
}

### Response: 201 Created
# {
#   "id": "abc123...",
#   "email": "student@example.com",
#   "fullName": "Jane Doe",
#   "roles": [],
#   "emailConfirmed": false
# }
```

## Key Files to Understand

1. **Program.cs**: ASP.NET Core startup, Identity configuration, service registration
2. **ApplicationDbContext.cs**: EF Core DbContext with User, Role, UserRole entities
3. **User.cs**, **Role.cs**: Entity definitions extending IdentityUser/IdentityRole
4. **AuthenticationService.cs**: Registration, login, token generation logic
5. **UserService.cs**: Profile management, update, password change
6. **RoleService.cs**: Role assignment, removal, RBAC checks
7. **AuthController.cs**, **UsersController.cs**, **RolesController.cs**: REST endpoints
8. **IdentityBroker.cs**: EF Core queries, Identity framework integration
9. **Migrations/**: Database schema creation (auto-generated by EF Core)

## Running the Feature

### Prerequisites
- .NET 10.0 SDK installed
- SQL Server LocalDB installed (or PostgreSQL for production testing)
- Visual Studio 2022 or VS Code with C# extension

### Setup

```bash
cd Markwell.Core

# Install dependencies
dotnet restore

# Create & apply migrations
dotnet ef database update

# Run application
dotnet run

# Server listens on http://localhost:5129
```

### Testing

```bash
# Run all unit tests
dotnet test Tests/

# Run specific test file
dotnet test Tests/Unit/Services/UserServiceTests.cs

# Run with verbose output
dotnet test --verbosity detailed
```

### Testing Endpoints (via Scalar UI or curl)

```bash
# Access Scalar UI
curl http://localhost:5129/scalar/v1

# Or test endpoint directly
curl -X POST http://localhost:5129/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"SecurePass123!","fullName":"Test User"}'
```

## Common Errors & Solutions

| Error | Cause | Solution |
|-------|-------|----------|
| "Email format is invalid" | Malformed email | Provide valid RFC 5322 email |
| "Password must contain uppercase..." | Weak password | Add uppercase, lowercase, digit, special char |
| "Email is already registered" | Email exists | Use unique email or reset account |
| "Email not confirmed" | User never verified | Click confirmation link in email |
| "Unauthorized" | Missing JWT token | Include `Authorization: Bearer <token>` header |
| "Forbidden" | Insufficient role | User lacks required role (Admin, Manager) |
| "User not found" | Invalid user ID | Verify user ID exists; check typos |

## Next Steps After Implementation

1. **Performance Testing**: Load test with 1000+ concurrent users
2. **Security Audit**: Review password hashing, token expiration, CORS
3. **Rate Limiting**: Implement on login/register to prevent brute force
4. **Email Integration**: Configure SMTP for verification emails
5. **Monitoring**: Add logging for authentication failures, role changes
6. **Documentation**: Generate OpenAPI/Swagger docs from contracts

## Links

- **Specification**: [spec.md](spec.md)
- **Data Model**: [data-model.md](data-model.md)
- **API Contracts**: [contracts/](contracts/)
- **Research & Decisions**: [research.md](research.md)
