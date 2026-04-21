# Data Model: Generic Storage Broker Integration

**Feature**: Generic Storage Broker Integration (`003-storage-broker-integration`)  
**Date**: April 17, 2026  
**Purpose**: Document entity definitions, broker structure, and model-to-broker mapping

---

## Entity Model Overview

```
┌─────────────────────────────────────────────────────────────┐
│                   Markwell.Core Entities                    │
└─────────────────────────────────────────────────────────────┘

User                          Role                      UserRole
├─ Id (string, PK)            ├─ Id (string, PK)        ├─ UserId (string, FK)
├─ Email (string)             ├─ Name (string)          ├─ RoleId (string, FK)
├─ UserName (string)          ├─ Description (string)   ├─ AssignedOn (DateTime)
├─ PasswordHash (string)       ├─ NormalizedName (string)├─ AssignedBy (string, FK)
├─ EmailConfirmed (bool)       └─ ConcurrencyStamp      └─ ConcurrencyStamp
├─ PhoneNumber (string)
├─ PhoneNumberConfirmed (bool)
├─ TwoFactorEnabled (bool)
├─ LockoutEnd (DateTime?)
├─ LockoutEnabled (bool)
├─ AccessFailedCount (int)
├─ ConcurrencyStamp (string)
└─ SecurityStamp (string)

Relationships:
  User ──(1..n)──> UserRole
  Role ──(1..n)──> UserRole
```

---

## StorageBroker Configuration

### Configuration via DbContext

Configuration is handled by `ApplicationDbContext.OnConfiguring()`:

```csharp
public class ApplicationDbContext : IdentityDbContext<User, Role, string>
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configuration string validation happens here
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=MarkwellCore.db");
        }
    }
}
```

### StorageBroker Constructor

```csharp
public class StorageBroker
{
    private readonly ApplicationDbContext _dbContext;

    public StorageBroker(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
}
```

### Behavior

| Operation | Behavior | Exception Handling |
|-----------|----------|-------------------|
| Insert | Adds entity to DbContext, SaveChanges | Throws DbUpdateException (constraint violations) |
| Select | Returns IQueryable<T> for DbSet<T> | Never throws; returns empty if no matches |
| SelectById | Calls DbSet<T>.FindAsync(id) | Returns null if not found; never throws |
| Update | Marks entity as Modified, SaveChanges | Throws DbUpdateException or DbUpdateConcurrencyException |
| Delete | Finds entity, removes, SaveChanges | Throws DbUpdateException if entity not found |

---

## Broker-to-Entity Mapping

---

## Broker Architecture

### IProfileBroker: User & Role Profile Operations Interface

**Purpose**: Domain-specific broker interface for all user, role, and profile management operations.  
**Responsibility**: Define contract for user/role CRUD, profile queries, and role assignments.

```csharp
public interface IProfileBroker
{
    // User Operations
    Task<User> CreateUserAsync(User user);
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByUserNameAsync(string userName);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User> UpdateUserAsync(User user);
    Task DeleteUserAsync(string userId);

    // Role Operations
    Task<Role> CreateRoleAsync(Role role);
    Task<Role?> GetRoleByIdAsync(string roleId);
    Task<Role?> GetRoleByNameAsync(string name);
    Task<IEnumerable<Role>> GetPredefinedRolesAsync();
    Task<Role> UpdateRoleAsync(Role role);
    Task DeleteRoleAsync(string roleId);

    // Profile & Role Assignment Operations
    Task<User?> GetUserWithRolesAsync(string userId);
    Task<IEnumerable<UserRole>> GetUserRolesAsync(string userId);
    Task<IEnumerable<UserRole>> GetRoleUsersAsync(string roleId);
    Task<UserRole?> GetUserRoleAsync(string userId, string roleId);
    Task AssignRoleToUserAsync(string userId, string roleId, string assignedByUserId);
    Task RemoveRoleFromUserAsync(string userId, string roleId);
}
```

### StorageBroker: Generic CRUD Implementation

**Purpose**: Provides generic CRUD operations for all entity types.  
**Responsibility**: Entity persistence, exception handling, concurrency control.

```csharp
public class StorageBroker
{
    private readonly ApplicationDbContext _dbContext;

    public StorageBroker(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Generic CRUD Operations (with exception handling)
    public Task<T> InsertAsync<T>(T entity) where T : class { /* implementation */ }
    public IQueryable<T> Select<T>() where T : class { /* implementation */ }
    public Task<T?> SelectByIdAsync<T>(string id) where T : class { /* implementation */ }
    public Task<T> UpdateAsync<T>(T entity) where T : class { /* implementation */ }
    public Task DeleteAsync<T>(string id) where T : class { /* implementation */ }
}
```

### ProfileBroker: User & Role Profile Implementation

**Purpose**: Domain-specific broker implementing IProfileBroker.  
**Responsibility**: User, role, and profile operations with business logic.

```csharp
public class ProfileBroker : IProfileBroker
{
    private readonly StorageBroker _storageBroker;

    public ProfileBroker(StorageBroker storageBroker)
    {
        _storageBroker = storageBroker;
    }

    // User Operations (delegates to StorageBroker + business logic)
    public async Task<User> CreateUserAsync(User user)
        => await _storageBroker.InsertAsync(user);

    public async Task<User?> GetUserByIdAsync(string userId)
        => await _storageBroker.SelectByIdAsync<User>(userId);

    public async Task<User?> GetUserByEmailAsync(string email)
        => await _storageBroker.Select<User>()
            .FirstOrDefaultAsync(u => u.Email == email);

    // ... other profile operations
}
```

## Broker Implementation Strategy

### One Broker for Profile Operations

A single **ProfileBroker** implementation of **IProfileBroker** handles all user, role, and profile management:

| Responsibility | Broker | Interface |
|---|---|---|
| User CRUD, role assignment, profile queries | ProfileBroker | IProfileBroker |

**Constraint**: One interface (IProfileBroker), one implementation (ProfileBroker). All profile-related operations consolidated in one place.

---

## StorageBroker CRUD Methods

StorageBroker provides generic CRUD operations that ProfileBroker uses internally:

```csharp
public class StorageBroker
{
    /// <summary>
    /// Inserts a new entity into the database.
    /// </summary>
    /// <typeparam name="T">The entity type to insert (e.g., User, Role)</typeparam>
    /// <param name="entity">The entity instance to insert</param>
    /// <returns>The inserted entity with generated ID and ConcurrencyStamp</returns>
    /// <exception cref="DbUpdateException">On constraint violation or database error</exception>
    public async Task<T> InsertAsync<T>(T entity) where T : class { /* implementation */ }

    /// <summary>
    /// Retrieves all entities of the specified type from the database.
    /// </summary>
    /// <typeparam name="T">The entity type to retrieve</typeparam>
    /// <returns>IQueryable of all entities; supports further filtering/ordering</returns>
    public IQueryable<T> Select<T>() where T : class { /* implementation */ }

    /// <summary>
    /// Retrieves a single entity by its ID.
    /// </summary>
    /// <typeparam name="T">The entity type to retrieve</typeparam>
    /// <param name="id">The entity ID</param>
    /// <returns>The entity if found; null otherwise</returns>
    public async Task<T?> SelectByIdAsync<T>(string id) where T : class { /* implementation */ }

    /// <summary>
    /// Updates an existing entity in the database.
    /// </summary>
    /// <typeparam name="T">The entity type to update</typeparam>
    /// <param name="entity">The entity instance with updated values</param>
    /// <returns>The updated entity</returns>
    /// <exception cref="DbUpdateException">On constraint violation or entity not found</exception>
    /// <exception cref="DbUpdateConcurrencyException">On ConcurrencyStamp mismatch</exception>
    public async Task<T> UpdateAsync<T>(T entity) where T : class { /* implementation */ }

    /// <summary>
    /// Deletes an entity from the database by ID.
    /// </summary>
    /// <typeparam name="T">The entity type to delete</typeparam>
    /// <param name="id">The entity ID to delete</param>
    /// <exception cref="DbUpdateException">If entity not found or database error</exception>
    public async Task DeleteAsync<T>(string id) where T : class { /* implementation */ }
}
```

---

## ProfileBroker: Domain Implementation

**Location**: `Markwell.Core/Brokers/ProfileBroker.cs`  
**Implements**: IProfileBroker  
**Responsibility**: All user, role, and profile operations via wrapped StorageBroker

**Predefined Roles**:
```csharp
Admin      // Full application access
Manager    // Management and reporting access
Teacher    // Course instruction access
Student    // Student portal access
```

**Implementation Pattern**:
```csharp
public class ProfileBroker : IProfileBroker
{
    private readonly StorageBroker _storageBroker;

    public ProfileBroker(StorageBroker storageBroker)
    {
        _storageBroker = storageBroker;
    }

    // User Operations
    public async Task<User> CreateUserAsync(User user)
        => await _storageBroker.InsertAsync(user);

    public async Task<User?> GetUserByIdAsync(string userId)
        => await _storageBroker.SelectByIdAsync<User>(userId);

    public async Task<User?> GetUserByEmailAsync(string email)
        => await _storageBroker.Select<User>()
            .FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetUserByUserNameAsync(string userName)
        => await _storageBroker.Select<User>()
            .FirstOrDefaultAsync(u => u.UserName == userName);

    public async Task<IEnumerable<User>> GetAllUsersAsync()
        => await _storageBroker.Select<User>().ToListAsync();

    public async Task<User> UpdateUserAsync(User user)
        => await _storageBroker.UpdateAsync(user);

    public async Task DeleteUserAsync(string userId)
        => await _storageBroker.DeleteAsync<User>(userId);

    // Role Operations
    public async Task<Role> CreateRoleAsync(Role role)
        => await _storageBroker.InsertAsync(role);

    public async Task<Role?> GetRoleByIdAsync(string roleId)
        => await _storageBroker.SelectByIdAsync<Role>(roleId);

    public async Task<Role?> GetRoleByNameAsync(string name)
        => await _storageBroker.Select<Role>()
            .FirstOrDefaultAsync(r => r.Name == name);

    public async Task<IEnumerable<Role>> GetPredefinedRolesAsync()
        => await _storageBroker.Select<Role>()
            .Where(r => new[] { "Admin", "Manager", "Teacher", "Student" }.Contains(r.Name))
            .ToListAsync();

    public async Task<Role> UpdateRoleAsync(Role role)
        => await _storageBroker.UpdateAsync(role);

    public async Task DeleteRoleAsync(string roleId)
        => await _storageBroker.DeleteAsync<Role>(roleId);

    // Profile & Role Assignment Operations
    public async Task<User?> GetUserWithRolesAsync(string userId)
        => await _storageBroker.Select<User>()
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId);

    public async Task<IEnumerable<UserRole>> GetUserRolesAsync(string userId)
        => await _storageBroker.Select<UserRole>()
            .Where(ur => ur.UserId == userId)
            .ToListAsync();

    public async Task<IEnumerable<UserRole>> GetRoleUsersAsync(string roleId)
        => await _storageBroker.Select<UserRole>()
            .Where(ur => ur.RoleId == roleId)
            .ToListAsync();

    public async Task<UserRole?> GetUserRoleAsync(string userId, string roleId)
        => await _storageBroker.Select<UserRole>()
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

    public async Task AssignRoleToUserAsync(string userId, string roleId, string assignedByUserId)
    {
        var userRole = new UserRole 
        { 
            UserId = userId, 
            RoleId = roleId, 
            AssignedOn = DateTime.UtcNow,
            AssignedBy = assignedByUserId
        };
        await _storageBroker.InsertAsync(userRole);
    }

    public async Task RemoveRoleFromUserAsync(string userId, string roleId)
    {
        var userRole = await GetUserRoleAsync(userId, roleId);
        if (userRole != null)
            await _storageBroker.DeleteAsync<UserRole>(userRole.Id);
    }
}
```

---

## Configuration Injection Strategy

### ApplicationSettings (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MarkwellCore;User Id=postgres;Password=password"
  }
}
```

### Environment-Specific Overrides

**appsettings.Development.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=MarkwellCore.db"
  }
}
```

**appsettings.Production.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-db-server;Database=MarkwellCore;..."
  }
}
```

### StorageBroker Configuration Load

```csharp
public StorageBroker(ApplicationDbContext dbContext, IConfiguration configuration)
{
    _dbContext = dbContext;
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    
    // Connection string is now available to DbContext
    // (DbContext is pre-configured by Program.cs with UseSqlServer or UseSqlite)
}
```

---

## Dependency Injection Registration

### Program.cs Service Registration

```csharp
// DbContext registration
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=MarkwellCore.db")
);

// Generic CRUD implementation
services.AddScoped<StorageBroker>();

// Profile-specific broker (user, role, profile operations)
services.AddScoped<IProfileBroker, ProfileBroker>();
```

### Service Consumption Pattern

```csharp
public class UserManagementService
{
    private readonly IProfileBroker _profileBroker;

    public UserManagementService(IProfileBroker profileBroker)
    {
        _profileBroker = profileBroker;
    }

    public async Task<User?> GetUserAsync(string userId)
        => await _profileBroker.GetUserByIdAsync(userId);

    public async Task<User> CreateUserAsync(User user)
        => await _profileBroker.CreateUserAsync(user);

    public async Task<User?> FindUserByEmailAsync(string email)
        => await _profileBroker.GetUserByEmailAsync(email);

    public async Task AssignRoleAsync(string userId, string roleId, string assignedByUserId)
        => await _profileBroker.AssignRoleToUserAsync(userId, roleId, assignedByUserId);
}
```

---

## Data Flow Diagram

```
HTTP Request
     ↓
 Controller (UsersController)
     ↓
 Orchestration Service (ProfileManagementOrchestrationService)
     ↓
 Business Service (UserService)
     ↓
 Profile Broker (IProfileBroker → ProfileBroker)
     ↓
 Generic CRUD (StorageBroker)
     ↓
 ApplicationDbContext
     ↓
 Database (SQLite/PostgreSQL)
     ↓
 HTTP Response
```

---

## Entity Lifecycle: User Creation Example

```
1. POST /users (UsersController)
   ↓
2. CreateUserAsync(CreateUserRequest) (ProfileManagementOrchestrationService)
   ↓
3. CreateUserAsync(user, createdByUserId) (UserService)
   ↓
4. InsertAsync<User>(user) (StorageBroker)
   ↓
5. _dbContext.Users.AddAsync(user); SaveChangesAsync()
   ↓
6. Database INSERT INTO [Users] ...
   ↓
7. Return user with generated Id, ConcurrencyStamp
```

---

## Thread Safety & Concurrency

| Scenario | Mechanism | Result |
|----------|-----------|--------|
| Multiple concurrent requests | Scoped DbContext per request | ✅ Safe — each request has isolated DbContext |
| Database write conflicts | Optimistic concurrency (ConcurrencyStamp) | ✅ DbUpdateConcurrencyException thrown |
| Read operations | No locking needed | ✅ Safe — reads don't modify state |
| Connection pooling | EF Core built-in | ✅ Efficient — reuses connections |

---

## Design Verification Checklist

- ✅ IProfileBroker interface: Defines all user, role, and profile operations
- ✅ StorageBroker: Generic CRUD implementation with exception handling
- ✅ ProfileBroker: Single domain-specific broker implementing IProfileBroker
- ✅ ProfileBroker wraps StorageBroker to provide all profile operations
- ✅ One interface per broker rule enforced (IProfileBroker + ProfileBroker)
- ✅ Configuration handled by ApplicationDbContext.OnConfiguring()
- ✅ DI registration: StorageBroker, IProfileBroker → ProfileBroker
- ✅ Predefined roles (Admin, Manager, Teacher, Student) supported via ProfileBroker
- ✅ Scoped lifetime ensures thread-safe DbContext usage per request
- ✅ Constitution principles verified (layering, naming, method design)
- ✅ Exception handling: StorageBroker throws DbUpdateException, services handle
- ✅ No broker created without interface (IProfileBroker + ProfileBroker)
- ✅ No InitializeAsync needed — DbContext.OnConfiguring() handles configuration

**Status**: ✅ **DESIGN READY FOR TASK GENERATION**
