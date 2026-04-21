using Microsoft.AspNetCore.Identity;

namespace Markwell.Core.Entities;

/// <summary>
/// User entity extending ASP.NET Core Identity IdentityUser.
/// </summary>
public class User : IdentityUser
{
    /// <summary>
    /// Gets or sets the collection of roles assigned to this user.
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
