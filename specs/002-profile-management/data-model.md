# Data Model: Profile Management & RBAC

**Date**: April 16, 2026  
**Phase**: 1 - Design & Data Modeling  
**Feature**: Profile Management & Role-Based Access Control

## Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────┐
│ User (extends IdentityUser<string>)                 │
├─────────────────────────────────────────────────────┤
│ Id: string (PK) [Guid converted to string]          │
│ Email: string (unique, required)                    │
│ NormalizedEmail: string (index)                      │
│ UserName: string (required, unique)                 │
│ NormalizedUserName: string (index)                   │
│ PasswordHash: string (bcrypt, required)             │
│ SecurityStamp: string (IdentityUser)                │
│ EmailConfirmed: bool (default: false)               │
│ PhoneNumber: string (optional)                      │
│ PhoneNumberConfirmed: bool (default: false)         │
│ TwoFactorEnabled: bool (default: false) [out of MVP]│
│ LockoutEnd: DateTimeOffset? (IdentityUser)          │
│ LockoutEnabled: bool (IdentityUser)                 │
│ AccessFailedCount: int (IdentityUser)               │
│ FullName: string (required, 1-255 chars)            │
│ CreatedAt: DateTime (default: UtcNow)               │
│ UpdatedAt: DateTime? (nullable)                     │
│ LastLoginAt: DateTime? (nullable)                   │
│ IsActive: bool (default: true)                      │
│                                                      │
│ Navigation: ICollection<UserRole> UserRoles         │
└─────────────────────────────────────────────────────┘
        │
        │ (one-to-many)
        │
        ├──────────────────────────────┐
        │                              │
        ▼                              ▼
┌──────────────────────────┐  ┌──────────────────────────┐
│ UserRole (Junction)      │  │ Role (IdentityRole)      │
├──────────────────────────┤  ├──────────────────────────┤
│ UserId: string (FK)      │──│ Id: string (PK)          │
│ RoleId: string (FK)      │  │ Name: string (required)  │
│ AssignedAt: DateTime     │  │ NormalizedName: string   │
│ AssignedBy: string (FK)  │  │ ConcurrencyStamp: string │
│                          │  │ CreatedAt: DateTime      │
└──────────────────────────┘  │                          │
   (composite PK: UserId,     │ Navigation:              │
    RoleId)                   │ ICollection<UserRole>    │
                              │ UserRoles               │
                              └──────────────────────────┘

Predefined Roles (seeded at migration):
- Admin
- Manager
- Teacher
- Student
```

## Entity Details

### User

**Purpose**: Central user identity entity extending ASP.NET Core IdentityUser

**Attributes**:
- **Id** (string, PK): Unique identifier (GUID converted to string by Identity framework)
- **Email** (string, required, unique): User's email address; used for login
- **UserName** (string, required, unique): Username for login (can be same as email)
- **PasswordHash** (string, required): Bcrypt-hashed password; never null for active users
- **FullName** (string, required): Display name; 1-255 characters
- **EmailConfirmed** (bool, default: false): Is email verified? Required before first login
- **PhoneNumber** (string, optional): User's phone number; optional field
- **PhoneNumberConfirmed** (bool, default: false): Is phone verified?
- **CreatedAt** (DateTime): Account creation timestamp (UTC); immutable after creation
- **UpdatedAt** (DateTime?, nullable): Last profile modification timestamp (UTC); null if never modified
- **LastLoginAt** (DateTime?, nullable): Most recent successful authentication; null if never logged in
- **IsActive** (bool, default: true): Soft-delete flag; false means account is deactivated but not physically deleted
- **SecurityStamp** (string, inherited): Used by Identity framework for sign-out-everywhere feature
- **LockoutEnd** (DateTimeOffset?, inherited): Lockout expiration time (for failed login attempts)
- **AccessFailedCount** (int, inherited): Count of failed login attempts

**Relationships**:
- Has many UserRoles (many-to-many to Roles)

**Validation**:
- Email: valid format (RFC 5322), max 256 chars, must be unique across system
- UserName: alphanumeric + underscore/hyphen, 3-50 chars, must be unique
- FullName: 1-255 chars, no leading/trailing whitespace
- PasswordHash: required for authenticated users; empty allowed during account creation (user must set password on first login)

**Soft Deletion**: IsActive=false indicates deactivated account; user cannot authenticate but data retained for audit trail

---

### Role

**Purpose**: Authorization level representing capabilities within Markwell system

**Predefined Values**:
1. **Admin**: Full system access
   - Can create, modify, delete users
   - Can assign any role to any user
   - Can modify other user passwords
   - Can view all system logs
   - Can manage role definitions

2. **Manager**: Team/school management
   - Can create users within their scope
   - Can modify users within their scope
   - Can assign Teacher/Student roles
   - Cannot assign Admin or Manager roles
   - Can view users in their scope

3. **Teacher**: Course instruction
   - Can create course content
   - Can grade assignments
   - Can view enrolled students
   - Can message students
   - Cannot modify other user accounts

4. **Student**: Learner role
   - Can enroll in courses
   - Can submit assignments
   - Can view grades and feedback
   - Can view course content
   - Cannot manage other users

**Attributes**:
- **Id** (string, PK): Unique identifier (GUID)
- **Name** (string, required, unique): Role name (Admin, Manager, Teacher, Student)
- **NormalizedName** (string, indexed): Uppercase name for case-insensitive lookups
- **ConcurrencyStamp** (string): EF Core optimistic concurrency token
- **CreatedAt** (DateTime): Role creation timestamp

**Relationships**:
- Has many UserRoles (many-to-many to Users)

**Notes**: 
- Roles are predefined; no dynamic role creation in MVP
- Role names are case-insensitive (normalized to uppercase for queries)
- All four roles must exist in the database; migration fails if any are missing

---

### UserRole

**Purpose**: Junction table for many-to-many relationship between User and Role; tracks assignment metadata

**Attributes**:
- **UserId** (string, FK to User.Id): Reference to user
- **RoleId** (string, FK to Role.Id): Reference to role
- **AssignedAt** (DateTime): When role was assigned (UTC)
- **AssignedBy** (string?, FK to User.Id, nullable): ID of admin who assigned role (nullable for seed data)

**Primary Key**: Composite key (UserId, RoleId) ensures no duplicate role assignments

**Relationships**:
- References User (required)
- References Role (required)
- References User (nullable) via AssignedBy for audit trail

**Constraints**:
- No duplicate (UserId, RoleId) pairs (enforced by composite PK)
- UserId and RoleId must reference existing User/Role records
- Cascading delete: if User is deleted, all UserRole entries for that user are deleted

---

## Data Constraints & Validation

### User Creation
- Email must be valid RFC 5322 format
- Email must be unique (case-insensitive)
- FullName must not be empty
- At registration time, PasswordHash can be null; user must set password on first login
- At least one role must be assigned

### Email Verification
- EmailConfirmed defaults to false
- User cannot authenticate until EmailConfirmed is true (exception: admin-created users can skip email verification or have it pre-confirmed)
- Verification token valid for 24 hours
- Token is single-use; can be regenerated

### Password Requirements
- Minimum 8 characters
- At least one uppercase letter (A-Z)
- At least one lowercase letter (a-z)
- At least one digit (0-9)
- At least one special character (@, #, $, %, ^, &, *, etc.)
- Cannot be same as username or previous password (if history tracked)
- Hashed using PBKDF2-SHA256 (Identity framework default)

### Role Assignment
- User must have at least one role
- Duplicate role assignments prevented by composite PK
- Admin role grants all permissions; no additional role needed
- Multiple roles per user allowed (e.g., Teacher + Manager)

### State Transitions
- **Registration**: User created with EmailConfirmed=false, IsActive=true
- **Email Verified**: User clicks verification link; EmailConfirmed=true
- **First Login**: User authenticates successfully; LastLoginAt set
- **Password Change**: UpdatedAt timestamp updated; SecurityStamp updated to invalidate existing tokens
- **Deactivation**: IsActive=false; user cannot authenticate; data retained
- **Role Change**: AssignedAt updated; authorization checked on next request

---

## Indexes & Query Performance

**Required Indexes**:
- `User.NormalizedEmail` - Fast email lookups during login
- `User.NormalizedUserName` - Fast username lookups
- `User.CreatedAt` - Sorting by creation date for admin dashboards
- `UserRole.UserId` - Fast role discovery per user
- `UserRole.RoleId` - Fast user discovery per role (admin queries)
- `Role.NormalizedName` - Fast role lookups (e.g., "ADMIN")

**Query Patterns**:
- Find user by email: `Users.SingleOrDefault(u => u.NormalizedEmail == normalizedEmail)`
- Find user roles: `UserRoles.Where(ur => ur.UserId == userId).Include(ur => ur.Role)`
- Find admin users: `Users.Include(u => u.UserRoles).Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Admin"))`
- Pagination for user list: `Users.OrderByDescending(u => u.CreatedAt).Skip(n).Take(pageSize)`

---

## EF Core Configuration

**DbContext Setup** (Provider-agnostic):
```csharp
public class ApplicationDbContext : IdentityDbContext<User, Role, string> {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        
        // Seed predefined roles
        var adminRole = new Role { Id = "1", Name = "Admin", NormalizedName = "ADMIN", CreatedAt = DateTime.UtcNow };
        var managerRole = new Role { Id = "2", Name = "Manager", NormalizedName = "MANAGER", CreatedAt = DateTime.UtcNow };
        var teacherRole = new Role { Id = "3", Name = "Teacher", NormalizedName = "TEACHER", CreatedAt = DateTime.UtcNow };
        var studentRole = new Role { Id = "4", Name = "Student", NormalizedName = "STUDENT", CreatedAt = DateTime.UtcNow };
        
        modelBuilder.Entity<Role>().HasData(adminRole, managerRole, teacherRole, studentRole);
        
        // Configure indexes (provider-agnostic)
        modelBuilder.Entity<User>().HasIndex(u => u.NormalizedEmail).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.CreatedAt);
        modelBuilder.Entity<Role>().HasIndex(r => r.NormalizedName).IsUnique();
    }
}
```

**Program.cs Configuration** (Environment-specific provider selection):
```csharp
// Local Development: SQLite in-memory (no external dependencies)
if (app.Environment.IsDevelopment()) {
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite("Data Source=:memory:"));
}

// Production: PostgreSQL with connection string from configuration
else {
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
```

---

## Migration Strategy

**Database Providers**:
- **Local Development**: SQLite in-memory database
  - Zero configuration; auto-created on startup via `context.Database.EnsureCreated()`
  - Resets on application restart (ideal for rapid development and testing)
  - No external database required

- **Integration Tests**: SQLite file-based (temporary)
  - Connection string: `Data Source=test_{Guid}.db;`
  - Cleaned up after test suite completion
  - Fast test execution; isolated per test

- **Development & Production**: PostgreSQL with code-first migrations
  - Development: Local PostgreSQL instance (Docker or native)
  - Production: PostgreSQL via environment variables
  - Both use identical migration path

**Initial Migration** (PostgreSQL):
- Command: `dotnet ef migrations add InitialCreate`
- Creates tables: `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`
- Identity framework tables: UserClaims, UserLogins, RoleClaims, UserTokens
- Seeds predefined roles (Admin, Manager, Teacher, Student)
- Deploy: `dotnet ef database update`

**Local Development** (SQLite in-memory):
- No migrations applied; schema created via `EnsureCreated()` on startup
- Automatic schema recreation on application restart
- Same `ApplicationDbContext` code; only connection string differs at runtime

**Zero-downtime Deployment** (PostgreSQL production):
- Use EF Core Code-First migrations with blue-green deployment strategy
- New schema deployed to standby environment
- Cutover: switch traffic after verification
- Rollback: revert traffic to previous database snapshot if issues detected
