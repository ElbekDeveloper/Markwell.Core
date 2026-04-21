using Markwell.Core.Entities;

namespace Markwell.Core.Brokers;

/// <summary>
/// Domain-specific broker interface for user, role, and profile management operations.
/// Provides 19 methods organized into User CRUD, Role CRUD, and Profile operations.
/// </summary>
public interface IProfileBroker
{
    // User CRUD Operations
    /// <summary>Creates a new user in the system.</summary>
    /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException">On constraint violation or database error</exception>
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
    /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException">On constraint violation or entity not found</exception>
    /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException">On ConcurrencyStamp mismatch</exception>
    Task<User> UpdateUserAsync(User user);

    /// <summary>Deletes a user by ID.</summary>
    /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException">If user not found</exception>
    Task DeleteUserAsync(string userId);

    // Role CRUD Operations
    /// <summary>Creates a new role in the system.</summary>
    /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException">On constraint violation or database error</exception>
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
    /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException">On constraint violation or entity not found</exception>
    Task<Role> UpdateRoleAsync(Role role);

    /// <summary>Deletes a role by ID.</summary>
    /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException">If role not found</exception>
    Task DeleteRoleAsync(string roleId);

    // Profile & Role Assignment Operations
    /// <summary>Retrieves a user with all assigned roles.</summary>
    /// <returns>User with roles included; null if user not found</returns>
    Task<User?> GetUserWithRolesAsync(string userId);

    /// <summary>Retrieves all roles assigned to a user.</summary>
    Task<IEnumerable<UserRole>> GetUserRolesAsync(string userId);

    /// <summary>Retrieves all users with a specific role.</summary>
    Task<IEnumerable<UserRole>> GetRoleUsersAsync(string roleId);

    /// <summary>Retrieves a specific user-role assignment.</summary>
    /// <returns>UserRole assignment if found; null otherwise</returns>
    Task<UserRole?> GetUserRoleAsync(string userId, string roleId);

    /// <summary>Assigns a role to a user.</summary>
    /// <param name="userId">The user ID</param>
    /// <param name="roleId">The role ID</param>
    /// <param name="assignedByUserId">The user ID who performed the assignment</param>
    /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException">On constraint violation or database error</exception>
    Task AssignRoleToUserAsync(string userId, string roleId, string assignedByUserId);

    /// <summary>Removes a role assignment from a user.</summary>
    /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException">If assignment not found</exception>
    Task RemoveRoleFromUserAsync(string userId, string roleId);
}

