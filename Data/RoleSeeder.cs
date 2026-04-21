using Markwell.Core.Brokers;
using Markwell.Core.Entities;

namespace Markwell.Core.Data;

/// <summary>
/// Seeder for predefined roles used across the Markwell.Core application.
/// Ensures that system roles (Admin, Manager, Teacher, Student) exist in the database.
/// Called during application startup to bootstrap role data.
/// </summary>
public static class RoleSeeder
{
    /// <summary>
    /// Seeds predefined roles into the database if they don't already exist.
    /// </summary>
    /// <param name="storageBroker">The storage broker for persisting roles</param>
    public static async Task SeedRolesAsync(StorageBroker storageBroker)
    {
        var predefinedRoles = new[]
        {
            new Role { Id = "admin", Name = "Admin", NormalizedName = "ADMIN", Description = "System administrator with full permissions" },
            new Role { Id = "manager", Name = "Manager", NormalizedName = "MANAGER", Description = "Manager role for organizational oversight" },
            new Role { Id = "teacher", Name = "Teacher", NormalizedName = "TEACHER", Description = "Educator role for instructional content" },
            new Role { Id = "student", Name = "Student", NormalizedName = "STUDENT", Description = "Student role for learning activities" }
        };

        foreach (var role in predefinedRoles)
        {
            var existingRole = await storageBroker.SelectByIdAsync<Role>(role.Id);
            if (existingRole == null)
            {
                await storageBroker.InsertAsync(role);
            }
        }
    }
}
