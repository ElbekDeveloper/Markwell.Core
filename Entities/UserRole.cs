using Microsoft.AspNetCore.Identity;

namespace Markwell.Core.Entities;

/// <summary>
/// UserRole entity for managing user-role assignments.
/// Extends IdentityUserRole to track assignment metadata.
/// </summary>
public class UserRole : IdentityUserRole<string>
{
    /// <summary>
    /// Gets or sets the unique identifier for this user-role assignment.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets when this role was assigned to the user.
    /// </summary>
    public DateTime AssignedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the user ID who performed the assignment.
    /// </summary>
    public string? AssignedBy { get; set; }

    /// <summary>
    /// Gets or sets the user navigation property.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the role navigation property.
    /// </summary>
    public Role? Role { get; set; }
}
