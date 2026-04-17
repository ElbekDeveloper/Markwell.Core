using Markwell.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Markwell.Core.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure User entity with custom properties
            builder.Entity<User>(entity =>
            {
                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsActive);
            });

            // Configure Role entity with custom properties
            builder.Entity<Role>(entity =>
            {
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .IsRequired();

                entity.HasIndex(e => e.Name);
            });

            // Configure the IdentityUserRole (AspNetUserRoles table) 
            // to support additional audit properties via UserRole shadow entity
            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("AspNetUserRoles");
            });

            // Seed predefined roles
            SeedRoles(builder);
        }

        private static void SeedRoles(ModelBuilder builder)
        {
            var now = DateTime.UtcNow;
            var roles = new[]
            {
                new Role
                {
                    Id = "role-admin",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    CreatedAt = now
                },
                new Role
                {
                    Id = "role-manager",
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                    CreatedAt = now
                },
                new Role
                {
                    Id = "role-teacher",
                    Name = "Teacher",
                    NormalizedName = "TEACHER",
                    CreatedAt = now
                },
                new Role
                {
                    Id = "role-student",
                    Name = "Student",
                    NormalizedName = "STUDENT",
                    CreatedAt = now
                }
            };

            builder.Entity<Role>().HasData(roles);
        }
    }
}
