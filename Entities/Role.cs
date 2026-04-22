using Microsoft.AspNetCore.Identity;

namespace Markwell.Core.Entities;

/// <summary>
/// Role entity extending ASP.NET Core Identity IdentityRole.
/// </summary>
public class Role : IdentityRole
{
    /// <summary>
    /// Gets or sets the description of the role's purpose and permissions.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the collection of user-role assignments.
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
