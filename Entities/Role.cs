using Microsoft.AspNetCore.Identity;

namespace Markwell.Core.Entities
{
    public class Role : IdentityRole<string>
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
