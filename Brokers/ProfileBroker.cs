using Microsoft.EntityFrameworkCore;
using Markwell.Core.Entities;

namespace Markwell.Core.Brokers;

/// <summary>
/// Domain-specific broker implementation for user, role, and profile management.
/// Wraps StorageBroker to provide consistent CRUD operations and profile queries.
/// Implements IProfileBroker interface with 19 methods across three categories:
/// User operations (7), Role operations (6), Profile operations (6).
/// </summary>
public class ProfileBroker : IProfileBroker
{
    private readonly StorageBroker _storageBroker;

    private static readonly string[] PredefinedRoleNames = ["ADMIN", "MANAGER", "TEACHER", "STUDENT"];

    /// <summary>
    /// Initializes a new instance of ProfileBroker.
    /// </summary>
    /// <param name="storageBroker">The underlying storage broker for CRUD operations</param>
    public ProfileBroker(StorageBroker storageBroker)
    {
        _storageBroker = storageBroker ?? throw new ArgumentNullException(nameof(storageBroker));
    }

    // ============================================
    // User CRUD Operations (7 methods)
    // ============================================

    /// <summary>Creates a new user in the system.</summary>
    public async Task<User> CreateUserAsync(User user)
        => await _storageBroker.InsertAsync(user);

    /// <summary>Retrieves a user by ID.</summary>
    public async Task<User?> GetUserByIdAsync(string userId)
        => await _storageBroker.SelectByIdAsync<User>(userId);

    /// <summary>Retrieves a user by email address.</summary>
    public async Task<User?> GetUserByEmailAsync(string email)
        => await _storageBroker.Select<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);

    /// <summary>Retrieves a user by username.</summary>
    public async Task<User?> GetUserByUserNameAsync(string userName)
        => await _storageBroker.Select<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == userName);

    /// <summary>Retrieves all users in the system.</summary>
    public async Task<IEnumerable<User>> GetAllUsersAsync()
        => await _storageBroker.Select<User>().AsNoTracking().ToListAsync();

    /// <summary>Updates an existing user.</summary>
    public async Task<User> UpdateUserAsync(User user)
        => await _storageBroker.UpdateAsync(user);

    /// <summary>Deletes a user by ID.</summary>
    public async Task DeleteUserAsync(string userId)
        => await _storageBroker.DeleteAsync<User>(userId);

    // ============================================
    // Role CRUD Operations (6 methods)
    // ============================================

    /// <summary>Creates a new role in the system.</summary>
    public async Task<Role> CreateRoleAsync(Role role)
        => await _storageBroker.InsertAsync(role);

    /// <summary>Retrieves a role by ID.</summary>
    public async Task<Role?> GetRoleByIdAsync(string roleId)
        => await _storageBroker.SelectByIdAsync<Role>(roleId);

    /// <summary>Retrieves a role by name.</summary>
    public async Task<Role?> GetRoleByNameAsync(string name)
        => await _storageBroker.Select<Role>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == name);

    /// <summary>Retrieves the four predefined roles (Admin, Manager, Teacher, Student).</summary>
    public async Task<IEnumerable<Role>> GetPredefinedRolesAsync()
        => await _storageBroker.Select<Role>()
            .AsNoTracking()
            .Where(r => PredefinedRoleNames.Contains(r.NormalizedName))
            .ToListAsync();

    /// <summary>Updates an existing role.</summary>
    public async Task<Role> UpdateRoleAsync(Role role)
        => await _storageBroker.UpdateAsync(role);

    /// <summary>Deletes a role by ID.</summary>
    public async Task DeleteRoleAsync(string roleId)
        => await _storageBroker.DeleteAsync<Role>(roleId);

    // ============================================
    // Profile & Role Assignment Operations (6 methods)
    // ============================================

    /// <summary>Retrieves a user with all assigned roles eager-loaded.</summary>
    public async Task<User?> GetUserWithRolesAsync(string userId)
        => await _storageBroker.Select<User>()
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

    /// <summary>Retrieves all roles assigned to a user.</summary>
    public async Task<IEnumerable<UserRole>> GetUserRolesAsync(string userId)
        => await _storageBroker.Select<UserRole>()
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .ToListAsync();

    /// <summary>Retrieves all users with a specific role.</summary>
    public async Task<IEnumerable<UserRole>> GetRoleUsersAsync(string roleId)
        => await _storageBroker.Select<UserRole>()
            .AsNoTracking()
            .Where(ur => ur.RoleId == roleId)
            .ToListAsync();

    /// <summary>Retrieves a specific user-role assignment.</summary>
    public async Task<UserRole?> GetUserRoleAsync(string userId, string roleId)
        => await _storageBroker.Select<UserRole>()
            .AsNoTracking()
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

    /// <summary>Assigns a role to a user. No-op if the assignment already exists.</summary>
    public async Task AssignRoleToUserAsync(string userId, string roleId, string assignedByUserId)
    {
        var existing = await GetUserRoleAsync(userId, roleId);
        if (existing is not null)
            return;

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedOn = DateTime.UtcNow,
            AssignedBy = assignedByUserId
        };

        await _storageBroker.InsertAsync(userRole);
    }

    /// <summary>Removes a role assignment from a user.</summary>
    /// <exception cref="DbUpdateException">If the assignment does not exist</exception>
    public async Task RemoveRoleFromUserAsync(string userId, string roleId)
    {
        var userRole = await _storageBroker.Select<UserRole>()
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId)
            ?? throw new DbUpdateException($"Role assignment for user '{userId}' and role '{roleId}' was not found.");

        await _storageBroker.RemoveAsync(userRole);
    }
}
