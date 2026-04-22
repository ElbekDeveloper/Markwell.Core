using Microsoft.EntityFrameworkCore;
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
    private static readonly Role[] PredefinedRoles =
    [
        new Role { Id = "admin",   Name = "Admin",   NormalizedName = "ADMIN",   Description = "System administrator with full permissions" },
        new Role { Id = "manager", Name = "Manager", NormalizedName = "MANAGER", Description = "Manager role for organizational oversight" },
        new Role { Id = "teacher", Name = "Teacher", NormalizedName = "TEACHER", Description = "Educator role for instructional content" },
        new Role { Id = "student", Name = "Student", NormalizedName = "STUDENT", Description = "Student role for learning activities" }
    ];

    /// <summary>
    /// Seeds predefined roles into the database if they don't already exist.
    /// Runs inside a transaction to guard against concurrent startup races.
    /// </summary>
    /// <param name="storageBroker">The storage broker for persisting roles</param>
    public static async Task SeedRolesAsync(StorageBroker storageBroker)
    {
        await using var transaction = await storageBroker.Database.BeginTransactionAsync();

        try
        {
            foreach (var role in PredefinedRoles)
            {
                var existing = await storageBroker.SelectByIdAsync<Role>(role.Id);
                if (existing is null)
                    await storageBroker.InsertAsync(role);
            }

            await transaction.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
        }
    }
}
