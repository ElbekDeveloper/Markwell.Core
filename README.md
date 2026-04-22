# Markwell.Core

A modern ASP.NET Core 10.0 Web API with identity management and a generic storage broker pattern for database operations.

## Architecture Overview

### Layered Architecture Pattern

Markwell.Core follows a three-layer architecture:

```
┌─────────────────────────────────────────┐
│       Controllers / HTTP Endpoints      │
├─────────────────────────────────────────┤
│        Services (Business Logic)        │
├─────────────────────────────────────────┤
│  Brokers (Data Access / Persistence)    │
├─────────────────────────────────────────┤
│    Database (SQLite / PostgreSQL)       │
└─────────────────────────────────────────┘
```

### Key Components

#### StorageBroker
Generic CRUD broker wrapping EntityFramework Core DbContext with support for:
- **Insert<T>**: Adds new entity to database
- **Select<T>**: Returns IQueryable for LINQ queries
- **SelectByIdAsync<T>(string id)**: Retrieves single entity by ID
- **UpdateAsync<T>**: Modifies existing entity
- **DeleteAsync<T>**: Removes entity by ID

**Thread Safety**: Scoped per HTTP request; DbContext is not shared across requests.

#### IProfileBroker / ProfileBroker
Domain-specific interface and implementation providing 19 methods organized as:

1. **User Operations** (7 methods)
   - CreateUserAsync, GetUserByIdAsync, GetUserByEmailAsync, GetUserByUserNameAsync, GetAllUsersAsync, UpdateUserAsync, DeleteUserAsync

2. **Role Operations** (6 methods)
   - CreateRoleAsync, GetRoleByIdAsync, GetRoleByNameAsync, GetPredefinedRolesAsync, UpdateRoleAsync, DeleteRoleAsync

3. **Profile & Role Assignment** (6 methods)
   - GetUserWithRolesAsync, GetUserRolesAsync, GetRoleUsersAsync, GetUserRoleAsync, AssignRoleToUserAsync, RemoveRoleFromUserAsync

### Predefined Roles

Four system roles are automatically seeded on startup:
- **Admin**: System administrator with full permissions
- **Manager**: Organizational oversight
- **Teacher**: Instructional content provider
- **Student**: Learning participant

## Usage Examples

### Injecting IProfileBroker

```csharp
public class UserManagementService
{
    private readonly IProfileBroker _profileBroker;

    public UserManagementService(IProfileBroker profileBroker)
    {
        _profileBroker = profileBroker;
    }
}
```

### Creating a User

```csharp
public async Task<User> CreateUserAsync(CreateUserRequest request)
{
    var user = new User
    {
        Id = Guid.NewGuid().ToString(),
        UserName = request.UserName,
        Email = request.Email,
        EmailConfirmed = true
    };
    
    return await _profileBroker.CreateUserAsync(user);
}
```

### Assigning Roles to Users

```csharp
public async Task AssignRoleAsync(string userId, string roleName, string assignedByUserId)
{
    var role = await _profileBroker.GetRoleByNameAsync(roleName);
    if (role != null)
    {
        await _profileBroker.AssignRoleToUserAsync(userId, role.Id, assignedByUserId);
    }
}
```

### Retrieving Users with Roles

```csharp
public async Task<UserProfileDto> GetUserProfileAsync(string userId)
{
    var user = await _profileBroker.GetUserWithRolesAsync(userId);
    if (user == null) return null;
    
    return new UserProfileDto
    {
        Id = user.Id,
        UserName = user.UserName,
        Email = user.Email,
        Roles = user.UserRoles?.Select(ur => ur.Role!.Name).ToList() ?? new()
    };
}
```

### Querying Users

```csharp
// Get user by ID
var user = await _profileBroker.GetUserByIdAsync("user-123");

// Get user by email
var userByEmail = await _profileBroker.GetUserByEmailAsync("john@example.com");

// Get all users
var allUsers = await _profileBroker.GetAllUsersAsync();
```

### Querying Roles

```csharp
// Get all predefined roles
var roles = await _profileBroker.GetPredefinedRolesAsync();

// Get specific role by name
var adminRole = await _profileBroker.GetRoleByNameAsync("Admin");
```

## Configuration

### Database Connection

Connection strings are configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=markwell.db"
  }
}
```

**Environment-Specific Configuration**:
- **Development** (`appsettings.Development.json`): Uses SQLite
- **Production** (`appsettings.Production.json`): Uses PostgreSQL

### Dependency Injection Setup

DI configuration in `Program.cs`:

```csharp
builder.Services.AddDbContext<StorageBroker>();
builder.Services.AddScoped<StorageBroker>();
builder.Services.AddScoped<IProfileBroker, ProfileBroker>();
```

Predefined roles are seeded automatically on startup.

## Building and Running

### Prerequisites
- .NET 10.0 SDK or higher
- PostgreSQL (production) or SQLite (development)

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run
```

### Health Check Endpoint
```bash
curl https://localhost:5001/health
```

## Entity Models

### User (ASP.NET Core Identity)
- Id, Email, UserName, PasswordHash
- EmailConfirmed, PhoneNumber, TwoFactorEnabled
- LockoutEnabled, AccessFailedCount
- UserRoles (navigation property)

### Role
- Id, Name, NormalizedName
- Description
- UserRoles (navigation property)

### UserRole (Junction Table)
- UserId (FK to User)
- RoleId (FK to Role)
- Id (assignment ID)
- AssignedOn (DateTime)
- AssignedBy (userId who performed assignment)

## Exception Handling

### StorageBroker Exception Contract

| Operation | Success | Not Found | Error |
|-----------|---------|-----------|-------|
| InsertAsync<T> | Returns entity | N/A | DbUpdateException |
| Select<T> | Returns IQueryable | Empty | Never |
| SelectByIdAsync<T> | Returns entity | **null** | Never |
| UpdateAsync<T> | Returns entity | DbUpdateException | DbUpdateException |
| DeleteAsync<T> | Task completes | Silently ignored | Silently ignored |

### IProfileBroker Exception Contract

Read operations (Get*) return null or empty collections on not found; write operations throw DbUpdateException on errors or not found.

## Concurrency Control

### Optimistic Concurrency

Each entity includes a `ConcurrencyStamp` field for detecting concurrent modifications:

```csharp
// Update will throw DbUpdateConcurrencyException if ConcurrencyStamp mismatch
try
{
    await _profileBroker.UpdateUserAsync(user);
}
catch (DbUpdateConcurrencyException)
{
    // Another request modified this user; re-read and retry
    var refreshedUser = await _profileBroker.GetUserByIdAsync(user.Id);
    // Merge changes and retry
}
```

## Performance

CRUD operations are optimized to complete within 100ms for typical queries against both in-memory (development) and production databases.

## Constitution & Standards

This project follows [Markwell.Core Constitution](specs/001-aspnet-webapi-setup/constitution.md):
- Singular broker names (StorageBroker, ProfileBroker)
- Interfaces prefixed with 'I' (IProfileBroker)
- Methods contain verbs (InsertAsync, SelectByIdAsync, etc.)
- Full testing discipline with unit and integration tests
- No direct commits to master; all changes via PR

## Documentation

- [Entity Data Model](specs/003-storage-broker-integration/data-model.md)
- [Broker Contract](specs/003-storage-broker-integration/contracts/broker-contract.md)
- [Implementation Plan](specs/003-storage-broker-integration/plan.md)
- [Specification](specs/003-storage-broker-integration/spec.md)