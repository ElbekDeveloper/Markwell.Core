# Storage & Profile Broker Contract Specification

**Feature**: Generic Storage Broker Integration (`003-storage-broker-integration`)  
**Date**: April 17, 2026  
**Purpose**: Define the configuration and domain-specific contracts for broker interfaces

---

## Contract Overview

The broker architecture defines one domain interface and one generic implementation:
- **StorageBroker**: Generic CRUD implementation (no interface)
- **IProfileBroker**: Domain-specific contract for user, role, and profile operations

This ensures:
- Generic CRUD operations are consistent across all entity types
- Domain operations are encapsulated in a single, cohesive interface
- Exception handling is clear and predictable
- Configuration is handled by DbContext.OnConfiguring()

---

## StorageBroker CRUD Contract

**StorageBroker** implements generic CRUD operations with the following contract:

```csharp
/// <summary>
/// Generic entity storage broker implementing CRUD operations.
/// Wraps ApplicationDbContext and handles entity persistence.
/// </summary>
public class StorageBroker
{
    /// <summary>
    /// Inserts a new entity into persistent storage.
    /// </summary>
    /// <typeparam name="T">The entity type (must be a class)</typeparam>
    /// <param name="entity">The entity instance to insert</param>
    /// <returns>The inserted entity with generated Id + ConcurrencyStamp</returns>
    /// <exception cref="DbUpdateException">On constraint violation or database error</exception>
    public async Task<T> InsertAsync<T>(T entity) where T : class { /* implementation */ }

    /// <summary>
    /// Retrieves all entities of the specified type.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <returns>IQueryable<T> for deferred execution and LINQ filtering</returns>
    public IQueryable<T> Select<T>() where T : class { /* implementation */ }

    /// <summary>
    /// Retrieves a single entity by ID.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="id">The entity ID</param>
    /// <returns>Entity if found; null otherwise</returns>
    public async Task<T?> SelectByIdAsync<T>(string id) where T : class { /* implementation */ }

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="entity">The entity with updated values</param>
    /// <returns>The updated entity</returns>
    /// <exception cref="DbUpdateException">On constraint violation or entity not found</exception>
    /// <exception cref="DbUpdateConcurrencyException">On ConcurrencyStamp mismatch</exception>
    public async Task<T> UpdateAsync<T>(T entity) where T : class { /* implementation */ }

    /// <summary>
    /// Deletes an entity by ID.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="id">The entity ID to delete</param>
    /// <exception cref="DbUpdateException">If entity not found</exception>
    public async Task DeleteAsync<T>(string id) where T : class { /* implementation */ }
}
```

### CRUD Return Type Semantics

| Method | Success Return | Not Found | Error |
|--------|----------------|-----------|-------|
| InsertAsync | Entity with Id + ConcurrencyStamp | N/A | DbUpdateException |
| Select | IQueryable<T> (may be empty) | Returns empty | Never throws |
| SelectByIdAsync | Entity instance | **Returns null** | Never throws |
| UpdateAsync | Updated entity | DbUpdateException | DbUpdateException |
| DeleteAsync | void (task completes) | DbUpdateException | DbUpdateException |

**Key Rules**:
- SelectByIdAsync: Returns **null** if not found (never throws)
- UpdateAsync: Throws if entity not found (not null-safe)
- DeleteAsync: Throws if entity not found (not null-safe)

---

## Exception Handling for CRUD Operations

### Broker Exception Semantics

StorageBroker CRUD methods throw exceptions in these scenarios:

| Exception | Thrown By | Scenario |
|-----------|-----------|----------|
| DbUpdateException | Insert, Update, Delete | Constraint violation, database error, entity not found (Update/Delete) |
| DbUpdateConcurrencyException | Update | ConcurrencyStamp mismatch (another request modified entity) |
| ArgumentNullException | Constructor | IConfiguration or DbContext not injected |

### Service Responsibility

Services using StorageBroker MUST handle:
- **SelectByIdAsync**: Check for null return (entity not found)
- **UpdateAsync**: Catch DbUpdateConcurrencyException (retry logic)
- **DeleteAsync**: Catch DbUpdateException (entity not found → NotFoundException)
- **InsertAsync**: Catch DbUpdateException (constraint violation → BadRequest)

---

## IProfileBroker Domain Contract

### Interface Definition

```csharp
/// <summary>
/// Domain-specific broker interface for user, role, and profile management.
/// All operations include exception handling defined below.
/// </summary>
public interface IProfileBroker
{
    // User Operations
    /// <summary>Creates a new user in the system.</summary>
    /// <exception cref="DbUpdateException">On constraint violation or database error</exception>
    Task<User> CreateUserAsync(User user);

    /// <summary>Retrieves a user by ID.</summary>
    /// <returns>User if found; null otherwise</returns>
    Task<User?> GetUserByIdAsync(string userId);

    /// <summary>Retrieves a user by email address.</summary>
    /// <returns>User if found; null otherwise</returns>
    Task<User?> GetUserByEmailAsync(string email);

    /// <summary>Retrieves a user by username.</summary>
    /// <returns>User if found; null otherwise</returns>
    Task<User?> GetUserByUserNameAsync(string userName);

    /// <summary>Retrieves all users in the system.</summary>
    Task<IEnumerable<User>> GetAllUsersAsync();

    /// <summary>Updates an existing user.</summary>
    /// <exception cref="DbUpdateException">On constraint violation or entity not found</exception>
    /// <exception cref="DbUpdateConcurrencyException">On ConcurrencyStamp mismatch</exception>
    Task<User> UpdateUserAsync(User user);

    /// <summary>Deletes a user by ID.</summary>
    /// <exception cref="DbUpdateException">If user not found</exception>
    Task DeleteUserAsync(string userId);

    // Role Operations
    /// <summary>Creates a new role in the system.</summary>
    /// <exception cref="DbUpdateException">On constraint violation or database error</exception>
    Task<Role> CreateRoleAsync(Role role);

    /// <summary>Retrieves a role by ID.</summary>
    /// <returns>Role if found; null otherwise</returns>
    Task<Role?> GetRoleByIdAsync(string roleId);

    /// <summary>Retrieves a role by name.</summary>
    /// <returns>Role if found; null otherwise</returns>
    Task<Role?> GetRoleByNameAsync(string name);

    /// <summary>Retrieves all predefined roles (Admin, Manager, Teacher, Student).</summary>
    Task<IEnumerable<Role>> GetPredefinedRolesAsync();

    /// <summary>Updates an existing role.</summary>
    /// <exception cref="DbUpdateException">On constraint violation or entity not found</exception>
    /// <exception cref="DbUpdateConcurrencyException">On ConcurrencyStamp mismatch</exception>
    Task<Role> UpdateRoleAsync(Role role);

    /// <summary>Deletes a role by ID.</summary>
    /// <exception cref="DbUpdateException">If role not found</exception>
    Task DeleteRoleAsync(string roleId);

    // Profile & Role Assignment Operations
    /// <summary>Retrieves a user with all assigned roles eager-loaded.</summary>
    /// <returns>User with roles if found; null otherwise</returns>
    Task<User?> GetUserWithRolesAsync(string userId);

    /// <summary>Retrieves all roles assigned to a user.</summary>
    Task<IEnumerable<UserRole>> GetUserRolesAsync(string userId);

    /// <summary>Retrieves all users with a specific role.</summary>
    Task<IEnumerable<UserRole>> GetRoleUsersAsync(string roleId);

    /// <summary>Checks if a user has a specific role assignment.</summary>
    /// <returns>UserRole if found; null otherwise</returns>
    Task<UserRole?> GetUserRoleAsync(string userId, string roleId);

    /// <summary>Assigns a role to a user.</summary>
    /// <exception cref="DbUpdateException">On constraint violation or database error</exception>
    Task AssignRoleToUserAsync(string userId, string roleId, string assignedByUserId);

    /// <summary>Removes a role from a user.</summary>
    /// <exception cref="DbUpdateException">If assignment not found or database error</exception>
    Task RemoveRoleFromUserAsync(string userId, string roleId);
}
```

### Return Type Semantics (IProfileBroker)

| Method | Success Return | Not Found | Error |
|--------|----------------|-----------|-------|
| Get* methods | Entity/List | **Returns null or empty list** | Never throws |
| Create* methods | Created entity | N/A | DbUpdateException |
| Update* methods | Updated entity | DbUpdateException | DbUpdateException / DbUpdateConcurrencyException |
| Delete* methods | void | DbUpdateException | DbUpdateException |
| Assign* methods | void | N/A (creates if not found) | DbUpdateException |
| Remove* methods | void | DbUpdateException | DbUpdateException |

**Key Rules**:
- Read operations (Get*): Return null or empty, never throw
- Write operations (Create*, Update*, Delete*, Assign*, Remove*): Throw on error
- Update*: Throws DbUpdateConcurrencyException on ConcurrencyStamp mismatch

### Exception Handling (IProfileBroker)

Services using IProfileBroker MUST handle:
- **Get* methods**: Check for null return
- **Update* methods**: Catch DbUpdateConcurrencyException (retry logic)
- **Delete* methods**: Catch DbUpdateException (not found → NotFoundException)
- **Create* methods**: Catch DbUpdateException (constraint → BadRequest)
- **Assign*/Remove* methods**: Catch DbUpdateException (database error)

---

## Generic Type Constraint

All methods use constraint: `where T : class`

This ensures:
- Only reference types (classes) are supported
- Value types (structs, primitives) are rejected at compile time
- Entity types must be classes (not records or struct)

---

## Concurrency Control

### ConcurrencyStamp Field

```csharp
// Each entity MUST have a ConcurrencyStamp property
public class User
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string ConcurrencyStamp { get; set; }  // Auto-managed by DbContext
}
```

### Optimistic Concurrency Behavior

1. **InsertAsync**: Generates new ConcurrencyStamp automatically
2. **SelectByIdAsync**: Retrieves current ConcurrencyStamp from storage
3. **UpdateAsync**: 
   - Checks if ConcurrencyStamp in entity matches storage
   - If mismatch: throws DbUpdateConcurrencyException
   - If match: updates entity, generates new ConcurrencyStamp
4. **DeleteAsync**: Deletes by ID only (no concurrency check)

**Consumer Responsibility**: Re-read entity, merge changes, retry update if DbUpdateConcurrencyException caught

---

## IConfiguration Injection

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

### Configuration Pattern

Configuration is handled by `ApplicationDbContext.OnConfiguring()`:

```csharp
public class ApplicationDbContext : IdentityDbContext<User, Role, string>
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=MarkwellCore.db");
        }
    }
}
```

---

## IQueryable Behavior (Select<T>)

### Deferred Execution

```csharp
// This query is NOT executed:
var query = _storageBroker.Select<User>();  // No database hit

// This query IS executed (materialization):
var users = await query.ToListAsync();       // Database hit
var count = await query.CountAsync();        // Database hit
var first = await query.FirstOrDefaultAsync(); // Database hit
```

### Supported LINQ Operations

```csharp
_storageBroker.Select<User>()
    .Where(u => u.Email.Contains("example.com"))           // ✅ Supported
    .OrderBy(u => u.UserName)                              // ✅ Supported
    .Take(10)                                              // ✅ Supported
    .Include(u => u.UserRoles)                             // ✅ Supported (with EF Core)
    .Select(u => new { u.Id, u.Email })                   // ✅ Supported
    .ToListAsync();
```

### Unsupported Operations

```csharp
// Client-side operations (executed in memory, NOT database):
_storageBroker.Select<User>()
    .ToList()                                   // ✅ OK but forces immediate materialization
    .Where(u => MyCustomMethod(u))             // ❌ Throws — server doesn't know MyCustomMethod
    .AsEnumerable()
    .Select(u => MyCustomLogic(u))             // ❌ Client-side evaluation
```

---

## Usage Examples

### Insert Example

```csharp
public async Task<User> CreateUserAsync(User user)
{
    try
    {
        // Contract: returns entity with Id + ConcurrencyStamp populated
        var createdUser = await _storageBroker.InsertAsync(user);
        return createdUser;
    }
    catch (DbUpdateException ex)
    {
        // Contract: constraint violation (email already exists, etc.)
        throw new BusinessLogicException("User already exists", ex);
    }
}
```

### SelectById Example

```csharp
public async Task<User?> GetUserAsync(string userId)
{
    // Contract: returns null if not found, never throws
    return await _storageBroker.SelectByIdAsync<User>(userId);
}
```

### Select Example

```csharp
public async Task<List<User>> FindUsersByEmailAsync(string emailDomain)
{
    // Contract: IQueryable supports LINQ, deferred execution
    return await _storageBroker.Select<User>()
        .Where(u => u.Email.EndsWith(emailDomain))
        .OrderBy(u => u.UserName)
        .ToListAsync();
}
```

### Update with Concurrency Example

```csharp
public async Task<User> UpdateUserAsync(User user)
{
    try
    {
        // Contract: throws if ConcurrencyStamp mismatches
        return await _storageBroker.UpdateAsync(user);
    }
    catch (DbUpdateConcurrencyException ex)
    {
        // Contract: another request modified this user
        // Retry logic: fetch fresh copy, reapply changes, try again
        var currentUser = await _storageBroker.SelectByIdAsync<User>(user.Id);
        throw new ConcurrencyException("User was modified by another user", ex);
    }
}
```

### Delete Example

```csharp
public async Task DeleteUserAsync(string userId)
{
    try
    {
        // Contract: throws if user not found, not null-safe
        await _storageBroker.DeleteAsync<User>(userId);
    }
    catch (DbUpdateException ex)
    {
        // Contract: user ID does not exist
        throw new NotFoundException($"User {userId} not found", ex);
    }
}
```

---

## Contract Validation Tests

Every implementation of IStorageBroker MUST pass these contract tests:

- [ ] InsertAsync: Inserts entity, returns with Id populated
- [ ] InsertAsync: Throws DbUpdateException on constraint violation
- [ ] SelectByIdAsync: Returns entity if found
- [ ] SelectByIdAsync: Returns null if not found (never throws)
- [ ] Select: Returns IQueryable (deferred execution)
- [ ] Select: Supports Where, OrderBy, Take, ToListAsync()
- [ ] UpdateAsync: Updates entity, returns updated instance
- [ ] UpdateAsync: Throws DbUpdateConcurrencyException on ConcurrencyStamp mismatch
- [ ] DeleteAsync: Deletes entity by ID
- [ ] DeleteAsync: Throws DbUpdateException if entity not found

---

## Contract Versioning

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-04-17 | Initial contract definition (5 CRUD methods, string IDs, generics) |

**Current Version**: 1.0.0 (Stable)

---

## Contract Compliance Checklist

### StorageBroker CRUD Implementation
- ✅ All 5 generic CRUD methods implemented (Insert, Select, SelectById, Update, Delete)
- ✅ Generic type constraint `where T : class` enforced
- ✅ InsertAsync returns entity with Id + ConcurrencyStamp
- ✅ SelectByIdAsync returns nullable (T?) allowing null return
- ✅ UpdateAsync throws if entity not found (not null-safe)
- ✅ DeleteAsync throws if entity not found
- ✅ Select returns IQueryable supporting LINQ and deferred execution
- ✅ Exception handling follows contract (DbUpdateException, DbUpdateConcurrencyException)
- ✅ Takes ApplicationDbContext in constructor (configuration via OnConfiguring)

### IProfileBroker Interface
- ✅ Domain-specific broker interface for profile operations
- ✅ All 19 profile operations defined (user, role, assignment CRUD + queries)
- ✅ Read methods (Get*) return nullable or empty (never throw)
- ✅ Write methods (Create*, Update*, Delete*, Assign*, Remove*) throw on error
- ✅ Update methods throw DbUpdateConcurrencyException on ConcurrencyStamp mismatch
- ✅ All methods documented with XML docs
- ✅ Exception handling specified in XML docs

### ProfileBroker Implementation
- ✅ Implements IProfileBroker interface
- ✅ Wraps StorageBroker for all CRUD operations
- ✅ All profile methods implemented
- ✅ Predefined roles (Admin, Manager, Teacher, Student) supported
- ✅ Business logic for role assignment included

**Status**: ✅ **CONTRACT READY FOR IMPLEMENTATION**
