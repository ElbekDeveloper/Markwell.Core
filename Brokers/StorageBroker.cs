using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Markwell.Core.Entities;

namespace Markwell.Core.Brokers;

/// <summary>
/// Application storage broker and database context providing both EntityFrameworkCore functionality
/// and domain-specific CRUD operations for all entity types.
/// Inherits from IdentityDbContext to manage User, Role, and UserRole entities with ASP.NET Identity.
/// Supports SQLite (development) and PostgreSQL (production).
/// </summary>
public class StorageBroker(DbContextOptions<StorageBroker> options, IConfiguration configuration)
    : IdentityDbContext<User, Role, string, IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>(options)
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection") ?? "Data Source=markwell.db";

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        if (!optionsBuilder.IsConfigured)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            if (environment == "Production")
                optionsBuilder.UseNpgsql(_connectionString);
            else
                optionsBuilder.UseSqlite(_connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    // Storage Broker CRUD Operations

    /// <summary>
    /// Inserts a new entity into persistent storage.
    /// </summary>
    /// <typeparam name="T">The entity type (must be a class)</typeparam>
    /// <param name="entity">The entity instance to insert</param>
    /// <returns>The inserted entity with generated Id and ConcurrencyStamp</returns>
    /// <exception cref="ArgumentNullException">If entity is null</exception>
    /// <exception cref="DbUpdateException">On constraint violation or database error</exception>
    public async Task<T> InsertAsync<T>(T entity) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        Add(entity);
        await SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Retrieves all entities of the specified type.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <returns>IQueryable of all entities; supports further filtering/ordering</returns>
    public IQueryable<T> Select<T>() where T : class
        => Set<T>();

    /// <summary>
    /// Retrieves a single entity by its string ID.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="id">The entity ID</param>
    /// <returns>Entity if found; null otherwise</returns>
    public async Task<T?> SelectByIdAsync<T>(string id) where T : class
        => await Set<T>().FindAsync(id);

    /// <summary>
    /// Updates an existing entity in the database.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="entity">The entity with updated values</param>
    /// <returns>The updated entity</returns>
    /// <exception cref="ArgumentNullException">If entity is null</exception>
    /// <exception cref="DbUpdateException">On constraint violation or entity not found</exception>
    /// <exception cref="DbUpdateConcurrencyException">On ConcurrencyStamp mismatch</exception>
    public async Task<T> UpdateAsync<T>(T entity) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        Update(entity);
        await SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Deletes an entity from the database by ID.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="id">The entity ID to delete</param>
    /// <exception cref="DbUpdateException">If entity not found</exception>
    public async Task DeleteAsync<T>(string id) where T : class
    {
        var entity = await SelectByIdAsync<T>(id)
            ?? throw new DbUpdateException($"Entity of type {typeof(T).Name} with id '{id}' was not found.");
        Remove(entity);
        await SaveChangesAsync();
    }

    /// <summary>
    /// Removes a tracked entity instance directly from the database.
    /// Use this for entities with composite primary keys (e.g., UserRole).
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="entity">The entity instance to remove</param>
    /// <exception cref="ArgumentNullException">If entity is null</exception>
    /// <exception cref="DbUpdateException">On database error</exception>
    public async Task RemoveAsync<T>(T entity) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        Remove(entity);
        await SaveChangesAsync();
    }
}
