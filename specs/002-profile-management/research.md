# Research & Design Decisions: Profile Management Feature

**Date**: April 16, 2026  
**Phase**: 0 - Research & Clarification Resolution  
**Feature**: Profile Management & Role-Based Access Control

## Research Tasks Completed

### 1. Database Choice for EF Core Identity

**NEEDS CLARIFICATION Resolved**: Storage database choice between SQL Server, PostgreSQL, or SQLite

**Decision: PostgreSQL (development + production) with SQLite for local/testing environments**

**Rationale**: 
- PostgreSQL provides consistent database across development and production (eliminates dev/prod parity issues)
- Open-source and portable; runs on Windows, Linux, macOS with identical behavior
- Excellent EF Core support with comprehensive tooling and migrations
- SQLite for local development and fast unit tests (in-memory, zero configuration, no external dependencies)
- Avoids SQL Server licensing and platform-specific tooling overhead
- Educational platform benefits from platform-independent solution

**Alternatives Considered**:
- **SQL Server LocalDB**: Rejected; Windows-specific, introduces dev/prod divergence
- **SQLite for production**: Rejected; lacks concurrent write support for production workloads
- **MariaDB/MySQL**: Not selected; PostgreSQL offers superior JSON support and features

**Implementation Approach**:
- **Local Development**: SQLite in-memory database for rapid iteration
  - Connection string: `Data Source=:memory:;`
  - Auto-created on startup via `database.EnsureCreated()`
  - Resets on application restart (suitable for development)

- **Integration Tests**: SQLite file-based (temporary, recreated per test suite)
  - Connection string: `Data Source=test_{Guid}.db;`
  - Cleaned up after test completion

- **Development & Production**: PostgreSQL with connection string management
  - Development: `Host=localhost;Database=markwell_core_dev;Username=postgres;Password=<local>;`
  - Production: Connection string from environment variables (managed by infrastructure)

- **EF Core Configuration**: Provider-agnostic DbContext with runtime provider selection

**Decision Artifact**: EF Core provider dependencies: `Npgsql.EntityFrameworkCore.PostgreSQL` (v10.0) for production and `Microsoft.EntityFrameworkCore.Sqlite` (v10.0) for local/testing

---

### 2. ASP.NET Core Identity Integration Architecture

**Decision: Extend ASP.NET Core Identity User & Role entities**

**Rationale**:
- ASP.NET Core Identity provides production-ready authentication, password hashing (bcrypt), token generation, email confirmation workflows out of the box
- Extending `IdentityUser` and `IdentityRole` base classes allows custom fields while maintaining framework compatibility
- Default `IdentityDbContext` handles schema generation, migrations, and relationship management automatically
- Reduces reimplementation risk; aligns with platform conventions

**Implementation Approach**:
```csharp
// Custom User entity extending IdentityUser
public class User : IdentityUser<string> {
    public string FullName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    public ICollection<UserRole> UserRoles { get; set; }
}

// Use default IdentityRole for roles (Admin, Manager, Teacher, Student)
public class Role : IdentityRole<string> {
    public DateTime CreatedAt { get; set; }
}

// Custom DbContext
public class ApplicationDbContext : IdentityDbContext<User, Role, string> {
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
}
```

---

### 3. Authentication Endpoint Implementation

**Decision: Standard REST conventions with `/register` and `/login` endpoints (User selection A confirmed)**

**Rationale**:
- Clear separation of concerns: registration (account creation) vs authentication (credential validation)
- Familiar to all developers; aligns with modern REST API patterns
- Simple to document and test
- Doesn't conflict with ASP.NET Core Identity internal endpoints

**Implementation Details**:
- `POST /auth/register` - Create new user account with email/password (returns 201 Created + user ID)
- `POST /auth/login` - Authenticate user with email/password (returns 200 OK + JWT token or session cookie)
- Both endpoints validate input, hash passwords, and persist via EF Core Identity brokers

---

### 4. Role Management Strategy

**Decision: Flat role structure with predefined roles (no hierarchy)**

**Rationale**:
- Educational platform roles (Admin, Manager, Teacher, Student) don't require inheritance hierarchy
- Flat structure simplifies authorization checks and reduces complexity
- Four predefined roles seeded at migration time; no dynamic role creation in MVP
- Role assignment is many-to-many: users can have multiple roles (teacher who is also admin)

**Predefined Roles**:
- `Admin` - Full system access; can create/modify/delete users and assign roles
- `Manager` - Manage users within their scope; cannot assign roles outside scope
- `Teacher` - Create and manage course content; view student submissions
- `Student` - Enroll in courses; submit assignments; view grades

---

### 5. Test Strategy for Identity Feature

**Decision: Three-tier test pyramid (unit + integration + acceptance)**

**Rationale**:
- EF Core Identity involves database persistence, encryption, and token generation — requires integration tests
- Acceptance tests (via .http files) provide regression testing and API contract validation
- Unit tests ensure business logic (validation, authorization) works independently

**Test Coverage Plan**:
1. **Unit Tests**: Password validation, role authorization checks, email validation logic (xUnit + Moq)
2. **Integration Tests**: EF Core Identity broker operations, user creation/update, role assignment (SQLite in-memory DB)
3. **Acceptance Tests**: HTTP request/response examples in .http files for all user stories

---

### 6. Password Security & Hashing

**Decision: Use ASP.NET Core Identity PasswordHasher<User> with PBKDF2-SHA256 (default)**

**Rationale**:
- Industry-standard password hashing algorithm, OWASP-compliant
- Automatically salted per-user; resistant to rainbow tables
- No plaintext password storage; no custom crypto needed
- Built into IdentityUser; minimal implementation effort

**Password Validation Requirements**:
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- At least one special character (@, #, $, %, ^, &, etc.)
- Not equal to username or previous passwords (if password history implemented)

---

### 7. Email Verification Workflow

**Decision: Send verification email with token link; user clicks to confirm**

**Rationale**:
- Standard practice for SaaS applications
- Prevents fake email addresses and typos
- Allows deferred email confirmation (register now, confirm later within 24 hours)
- ASP.NET Core Identity provides token generation APIs

**Token Design**:
- Generated via `UserManager.GenerateEmailConfirmationTokenAsync()`
- Single-use token; expires after 24 hours
- Token embedded in confirmation link: `/auth/confirm-email?userId={id}&token={encoded-token}`
- User clicks link; backend validates and marks email as confirmed

---

## Resolved Design Decisions

| Decision | Selection | Status |
|----------|-----------|--------|
| Storage Database | PostgreSQL (dev + prod) + SQLite (local + test) | ✅ Resolved |
| Identity Framework | ASP.NET Core Identity extending IdentityUser/IdentityRole | ✅ Resolved |
| Auth Endpoints | REST convention: POST /auth/register, POST /auth/login | ✅ Resolved (User input A) |
| Role Structure | Flat 4 predefined roles (Admin, Manager, Teacher, Student) | ✅ Resolved |
| Password Hashing | PBKDF2-SHA256 via PasswordHasher<User> | ✅ Resolved |
| Test Strategy | Unit (xUnit) + Integration (SQLite) + Acceptance (.http) | ✅ Resolved |
| Email Verification | Token-based with 24-hour expiration | ✅ Resolved |

## Next Steps

→ Phase 1: Data Model Design (data-model.md)
→ Phase 1: API Contract Definition (contracts/)
→ Phase 1: Agent Context Update
