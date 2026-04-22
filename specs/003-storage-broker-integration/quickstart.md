# Quickstart: Generic Storage Broker Integration

**Feature**: Generic Storage Broker Integration (`003-storage-broker-integration`)  
**Date**: April 17, 2026  
**Audience**: Developers implementing storage operations in Markwell.Core

---

## What is IProfileBroker?

**IProfileBroker**: Domain-specific interface for all user, role, and profile operations. Provides comprehensive CRUD and query methods for managing profiles, users, and roles.

**StorageBroker**: Generic CRUD implementation wrapper around ApplicationDbContext. Used internally by ProfileBroker.

Data flow:
```
Service Layer
    ↓
IProfileBroker (interface)
    ↓
ProfileBroker (implementation)
    ↓
StorageBroker (generic CRUD)
    ↓
ApplicationDbContext (EF Core)
    ↓
Database (PostgreSQL or SQLite)
```

**Benefits**:
- ✅ Consistent profile operations across the application
- ✅ Type-safe with entity-specific methods
- ✅ Easy to mock for unit tests
- ✅ Configuration handled by DbContext.OnConfiguring()

---

## Installation & Setup

### 1. Verify Dependencies (Already Installed)

```xml
<!-- Markwell.Core.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.0" />
```

### 2. Configure Program.cs

```csharp
// Program.cs

// DbContext setup (existing)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=MarkwellCore.db")
);

// Add storage brokers (new)
builder.Services.AddScoped<StorageBroker>();
builder.Services.AddScoped<IProfileBroker, ProfileBroker>();
```

### 3. Verify appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=MarkwellCore.db"
  }
}
```

**That's it!** No additional configuration needed.

---

## Basic Usage: Profile Operations

### Inject IProfileBroker into Your Service

```csharp
public class UserManagementService
{
    private readonly IProfileBroker _profileBroker;

    public UserManagementService(IProfileBroker profileBroker)
    {
        _profileBroker = profileBroker;
    }

    // Now use profile operations below...
}
```

### CREATE: Add a User

```csharp
public async Task<User> CreateUserAsync(User user)
{
    return await _profileBroker.CreateUserAsync(user);
}

// Usage:
var newUser = new User 
{ 
    UserName = "john.doe", 
    Email = "john@example.com" 
};
var createdUser = await _userService.CreateUserAsync(newUser);
Console.WriteLine($"Created user: {createdUser.Id}");
```

### READ: Get Users

```csharp
public async Task<User?> GetUserAsync(string userId)
{
    return await _profileBroker.GetUserByIdAsync(userId);
}

public async Task<User?> GetUserByEmailAsync(string email)
{
    return await _profileBroker.GetUserByEmailAsync(email);
}

public async Task<List<User>> GetAllUsersAsync()
{
    var users = await _profileBroker.GetAllUsersAsync();
    return users.ToList();
}

// Usage:
var user = await _userService.GetUserAsync("user-123");
if (user == null)
{
    Console.WriteLine("User not found");
}

var userByEmail = await _userService.GetUserByEmailAsync("john@example.com");
```

### UPDATE: Modify User

```csharp
public async Task<User> UpdateUserAsync(User user)
{
    try
    {
        return await _profileBroker.UpdateUserAsync(user);
    }
    catch (DbUpdateConcurrencyException)
    {
        // Another request modified this user simultaneously
        throw;
    }
}

// Usage:
var user = await _userService.GetUserAsync("user-123");
if (user != null)
{
    user.Email = "newemail@example.com";
    await _userService.UpdateUserAsync(user);
}
```

### DELETE: Remove User

```csharp
public async Task DeleteUserAsync(string userId)
{
    try
    {
        await _profileBroker.DeleteUserAsync(userId);
    }
    catch (DbUpdateException)
    {
        throw new NotFoundException($"User {userId} not found");
    }
}

// Usage:
await _userService.DeleteUserAsync("user-123");
```

---

## Role Management Operations

### CREATE: Add a Role

```csharp
public async Task<Role> CreateRoleAsync(Role role)
{
    return await _profileBroker.CreateRoleAsync(role);
}
```

### READ: Get Roles

```csharp
public async Task<Role?> GetRoleByNameAsync(string name)
{
    return await _profileBroker.GetRoleByNameAsync(name);
}

public async Task<List<Role>> GetPredefinedRolesAsync()
{
    var roles = await _profileBroker.GetPredefinedRolesAsync();
    return roles.ToList();
}

// Returns: Admin, Manager, Teacher, Student
```

### UPDATE: Modify Role

```csharp
public async Task<Role> UpdateRoleAsync(Role role)
{
    return await _profileBroker.UpdateRoleAsync(role);
}
```

### DELETE: Remove Role

```csharp
public async Task DeleteRoleAsync(string roleId)
{
    await _profileBroker.DeleteRoleAsync(roleId);
}
```

---

## Profile & Role Assignment Operations

### Get User with Roles

```csharp
public async Task<User?> GetUserWithRolesAsync(string userId)
{
    // User object includes all assigned roles
    return await _profileBroker.GetUserWithRolesAsync(userId);
}

// Usage:
var userProfile = await _userService.GetUserWithRolesAsync("user-123");
if (userProfile != null)
{
    foreach (var role in userProfile.UserRoles)
    {
        Console.WriteLine($"User has role: {role.RoleId}");
    }
}
```

### List All User Roles

```csharp
public async Task<List<UserRole>> GetUserRolesAsync(string userId)
{
    var roles = await _profileBroker.GetUserRolesAsync(userId);
    return roles.ToList();
}
```

### Assign Role to User

```csharp
public async Task AssignRoleAsync(string userId, string roleId, string assignedByUserId)
{
    await _profileBroker.AssignRoleToUserAsync(userId, roleId, assignedByUserId);
}

// Usage:
await _userService.AssignRoleAsync("user-123", "admin-role-id", "admin-user-id");
```

### Remove Role from User

```csharp
public async Task RemoveRoleAsync(string userId, string roleId)
{
    try
    {
        await _profileBroker.RemoveRoleFromUserAsync(userId, roleId);
    }
    catch (DbUpdateException)
    {
        throw new NotFoundException("Role assignment not found");
    }
}
```

### Check If User Has Role

```csharp
public async Task<bool> UserHasRoleAsync(string userId, string roleId)
{
    var userRole = await _profileBroker.GetUserRoleAsync(userId, roleId);
    return userRole != null;
}
```

---

## Advanced: Domain-Specific Queries

ProfileBroker provides domain-specific methods for common queries:

### Find Users by Email Domain (Example)

While ProfileBroker doesn't expose raw LINQ, it provides specific query methods:

```csharp
public async Task<User?> FindUserByEmailAsync(string email)
{
    return await _profileBroker.GetUserByEmailAsync(email);
}

// For more complex queries, create business logic in your service:
public async Task<List<User>> FindUsersFromDomainAsync(string emailDomain)
{
    var allUsers = await _profileBroker.GetAllUsersAsync();
    return allUsers
        .Where(u => u.Email.EndsWith(emailDomain))
        .OrderBy(u => u.UserName)
        .ToList();  // Client-side filtering for this use case
}
```

### Get Users with Specific Roles

```csharp
public async Task<List<User>> GetUsersWithRoleAsync(string roleId)
{
    var usersWithRole = await _profileBroker.GetRoleUsersAsync(roleId);
    return usersWithRole.Select(ur => ur /* need full user data */).ToList();
}
```

### Aggregate Operations

For complex aggregations, combine ProfileBroker queries with LINQ to Objects:

```csharp
public async Task<int> GetActiveUserCountAsync()
{
    var allUsers = await _profileBroker.GetAllUsersAsync();
    return allUsers.Count(u => !u.LockoutEnd.HasValue || u.LockoutEnd < DateTime.UtcNow);
}

public async Task<Dictionary<string, int>> GetRoleDistributionAsync()
{
    var allRoles = await _profileBroker.GetPredefinedRolesAsync();
    var distribution = new Dictionary<string, int>();
    
    foreach (var role in allRoles)
    {
        var usersWithRole = await _profileBroker.GetRoleUsersAsync(role.Id);
        distribution[role.Name] = usersWithRole.Count();
    }
    
    return distribution;
}
```

---

## Testing: Mocking IProfileBroker

### Unit Test with Moq

```csharp
[Fact]
public async Task GetUserAsync_WithValidId_ReturnsUser()
{
    // Arrange
    var profileBrokerMock = new Mock<IProfileBroker>();
    var testUser = new User { Id = "user-123", Email = "test@example.com" };
    
    profileBrokerMock
        .Setup(pb => pb.GetUserByIdAsync("user-123"))
        .ReturnsAsync(testUser);
    
    var userService = new UserManagementService(profileBrokerMock.Object);

    // Act
    var result = await userService.GetUserAsync("user-123");

    // Assert
    Assert.NotNull(result);
    Assert.Equal("test@example.com", result.Email);
    profileBrokerMock.Verify(pb => pb.GetUserByIdAsync("user-123"), Times.Once);
}

[Fact]
public async Task GetUserAsync_WithInvalidId_ReturnsNull()
{
    // Arrange
    var profileBrokerMock = new Mock<IProfileBroker>();
    profileBrokerMock
        .Setup(pb => pb.GetUserByIdAsync("nonexistent"))
        .ReturnsAsync((User?)null);
    
    var userService = new UserManagementService(profileBrokerMock.Object);

    // Act
    var result = await userService.GetUserAsync("nonexistent");

    // Assert
    Assert.Null(result);
}

[Fact]
public async Task AssignRoleAsync_CallsProfileBroker()
{
    // Arrange
    var profileBrokerMock = new Mock<IProfileBroker>();
    profileBrokerMock
        .Setup(pb => pb.AssignRoleToUserAsync("user-123", "admin", "admin-user"))
        .Returns(Task.CompletedTask);
    
    var userService = new UserManagementService(profileBrokerMock.Object);

    // Act
    await userService.AssignRoleAsync("user-123", "admin", "admin-user");

    // Assert
    profileBrokerMock.Verify(
        pb => pb.AssignRoleToUserAsync("user-123", "admin", "admin-user"), 
        Times.Once);
}
```

### Integration Test with Real DbContext

```csharp
[Fact]
public async Task CreateUserAsync_CreatesUserInDatabase()
{
    // Arrange
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase("test-db")
        .Options;
    
    using var context = new ApplicationDbContext(options);
    var storageBroker = new StorageBroker(context, configuration);
    var profileBroker = new ProfileBroker(storageBroker);
    
    var newUser = new User 
    { 
        UserName = "testuser", 
        Email = "test@example.com" 
    };

    // Act
    var createdUser = await profileBroker.CreateUserAsync(newUser);

    // Assert
    Assert.NotNull(createdUser.Id);
    var retrievedUser = await profileBroker.GetUserByIdAsync(createdUser.Id);
    Assert.Equal("test@example.com", retrievedUser?.Email);
}

[Fact]
public async Task AssignRoleToUserAsync_CreatesAssignment()
{
    // Arrange
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase("test-db")
        .Options;
    
    using var context = new ApplicationDbContext(options);
    var storageBroker = new StorageBroker(context, configuration);
    var profileBroker = new ProfileBroker(storageBroker);
    
    // Setup user and role
    var user = await profileBroker.CreateUserAsync(new User { UserName = "user1" });
    var role = await profileBroker.CreateRoleAsync(new Role { Name = "admin" });

    // Act
    await profileBroker.AssignRoleToUserAsync(user.Id, role.Id, "admin-user");

    // Assert
    var userRole = await profileBroker.GetUserRoleAsync(user.Id, role.Id);
    Assert.NotNull(userRole);
}
```

---

## Error Handling Patterns

### Handle Not Found (Get* Methods)

```csharp
var user = await _profileBroker.GetUserByIdAsync(userId);
if (user == null)
{
    return NotFound($"User {userId} not found");
}
// Continue with user object
```

### Handle Concurrency Conflicts (Update* Methods)

```csharp
try
{
    await _profileBroker.UpdateUserAsync(user);
}
catch (DbUpdateConcurrencyException)
{
    // Another request modified this user
    // Reload and retry
    var currentUser = await _profileBroker.GetUserByIdAsync(user.Id);
    // Merge changes and retry...
}
```

### Handle Database Errors (Create*, Delete*, Assign* Methods)

```csharp
try
{
    await _profileBroker.CreateUserAsync(user);
}
catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE"))
{
    return BadRequest("User with this email already exists");
}
catch (DbUpdateException ex)
{
    return StatusCode(500, "Database error occurred");
}

try
{
    await _profileBroker.DeleteUserAsync(userId);
}
catch (DbUpdateException)
{
    return NotFound($"User {userId} not found or cannot be deleted");
}

try
{
    await _profileBroker.AssignRoleToUserAsync(userId, roleId, assignedByUserId);
}
catch (DbUpdateException ex)
{
    return BadRequest("Could not assign role: user or role may not exist");
}
```

---

## Configuration Behavior

### Environment-Specific Connection Strings

```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=MarkwellCore.db"
  }
}

// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-db.example.com;Database=MarkwellCore;..."
  }
}
```

**Behavior**: StorageBroker reads from current environment's appsettings.json automatically via IConfiguration injection.

---

## Common Patterns

### Service Pattern with IProfileBroker Injection

```csharp
public class UserManagementService
{
    private readonly IProfileBroker _profileBroker;

    public UserManagementService(IProfileBroker profileBroker)
    {
        _profileBroker = profileBroker;
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        var user = new User 
        { 
            UserName = request.UserName, 
            Email = request.Email 
        };
        
        var createdUser = await _profileBroker.CreateUserAsync(user);
        
        return new UserResponse 
        { 
            Id = createdUser.Id, 
            Email = createdUser.Email 
        };
    }

    public async Task<UserProfileResponse> GetUserProfileAsync(string userId)
    {
        var userProfile = await _profileBroker.GetUserWithRolesAsync(userId);
        if (userProfile == null)
            throw new NotFoundException($"User {userId} not found");

        var availableRoles = await _profileBroker.GetPredefinedRolesAsync();
        
        return new UserProfileResponse 
        { 
            User = userProfile, 
            AvailableRoles = availableRoles.ToList(),
            AssignedRoles = userProfile.UserRoles.Select(ur => ur.RoleId).ToList()
        };
    }
}
```

### Orchestration Service Pattern (Multiple Domain Operations)

```csharp
public class ProfileManagementOrchestrationService
{
    private readonly IProfileBroker _profileBroker;

    public ProfileManagementOrchestrationService(IProfileBroker profileBroker)
    {
        _profileBroker = profileBroker;
    }

    public async Task<UserProfileResponse> RegisterUserWithRoleAsync(
        CreateUserRequest userRequest, 
        string roleName)
    {
        // Create user
        var user = new User 
        { 
            UserName = userRequest.UserName, 
            Email = userRequest.Email 
        };
        var createdUser = await _profileBroker.CreateUserAsync(user);

        // Get role
        var role = await _profileBroker.GetRoleByNameAsync(roleName);
        if (role == null)
            throw new NotFoundException($"Role {roleName} not found");

        // Assign role to user
        await _profileBroker.AssignRoleToUserAsync(createdUser.Id, role.Id, "system");

        // Return profile
        var userProfile = await _profileBroker.GetUserWithRolesAsync(createdUser.Id);
        
        return new UserProfileResponse 
        { 
            User = userProfile,
            AvailableRoles = (await _profileBroker.GetPredefinedRolesAsync()).ToList()
        };
    }
}
```

---

## Troubleshooting

### DbContext Not Registered

**Error**: `InvalidOperationException: Unable to resolve service for type 'ApplicationDbContext'`

**Solution**: Verify Program.cs has `services.AddDbContext<ApplicationDbContext>(...)`

```csharp
// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);
```

### StorageBroker Not Registered

**Error**: `InvalidOperationException: Unable to resolve service for type 'StorageBroker'`

**Solution**: Verify Program.cs registers StorageBroker:

```csharp
builder.Services.AddScoped<StorageBroker>();
```

### ProfileBroker Not Registered

**Error**: `InvalidOperationException: Unable to resolve service for type 'IProfileBroker'`

**Solution**: Verify Program.cs registers ProfileBroker:

```csharp
builder.Services.AddScoped<IProfileBroker, ProfileBroker>();
```

### Connection String Not Found

**Error**: `ArgumentNullException: Connection string 'DefaultConnection' not found.`

**Solution**: Check appsettings.json has ConnectionStrings section:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=MarkwellCore.db"
  }
}
```

### User/Role Not Found (Get* Methods)

**Error**: Unexpected null returns from Get* methods

**Solution**: Always check for null before using returned values:

```csharp
var user = await _profileBroker.GetUserByIdAsync(userId);
if (user == null)
{
    // Handle not found case
    return NotFound();
}
// Safe to use user
```

### Delete Operations Failing

**Error**: `DbUpdateException` when deleting users or roles

**Solution**: These operations throw exceptions when entity not found:

```csharp
try
{
    await _profileBroker.DeleteUserAsync(userId);
}
catch (DbUpdateException)
{
    throw new NotFoundException($"User {userId} not found");
}
```

### Concurrency Conflicts on Update

**Error**: `DbUpdateConcurrencyException` when updating user or role

**Solution**: Another request modified the entity simultaneously. Reload and retry:

```csharp
try
{
    await _profileBroker.UpdateUserAsync(user);
}
catch (DbUpdateConcurrencyException)
{
    var freshUser = await _profileBroker.GetUserByIdAsync(user.Id);
    // Re-merge your changes and retry
    await _profileBroker.UpdateUserAsync(freshUser);
}
```

---

## Next Steps

1. ✅ Read the [broker-contract.md](contracts/broker-contract.md) for detailed method specifications
2. ✅ Review [data-model.md](data-model.md) for entity relationships
3. ✅ Check tests in `Tests/Unit/Brokers/` for usage examples
4. ✅ Implement service layer consuming IProfileBroker

---

## Support

- **Issue**: File GitHub issue with `[storage-broker]` tag
- **Questions**: Refer to the Constitution at `.specify/memory/constitution.md`
- **Examples**: See `Tests/` directory for complete usage patterns

**Status**: ✅ **QUICKSTART COMPLETE — READY FOR IMPLEMENTATION**
