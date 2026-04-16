using Markwell.Core.Brokers;
using Markwell.Core.Entities;

namespace Markwell.Core.Services
{
    public class RoleService
    {
        private readonly RoleBroker _roleBroker;
        private readonly IdentityBroker _identityBroker;

        public RoleService(RoleBroker roleBroker, IdentityBroker identityBroker)
        {
            _roleBroker = roleBroker;
            _identityBroker = identityBroker;
        }

        public async Task AssignRoleAsync(string userId, string roleName, string assignedByUserId)
        {
            // Validate role exists
            var role = await _roleBroker.GetRoleByNameAsync(roleName);
            if (role == null)
                throw new ArgumentException($"Role '{roleName}' does not exist", nameof(roleName));

            // Get user
            var user = new User { Id = userId };

            // Check if user already has this role
            var userRoles = await _identityBroker.GetUserRolesAsync(user);
            if (userRoles.Contains(roleName))
                throw new InvalidOperationException($"User already has role '{roleName}'");

            // Assign role
            var result = await _identityBroker.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to assign role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        public async Task RemoveRoleAsync(string userId, string roleName, string removedByUserId)
        {
            // Validate role exists
            var role = await _roleBroker.GetRoleByNameAsync(roleName);
            if (role == null)
                throw new ArgumentException($"Role '{roleName}' does not exist", nameof(roleName));

            // Get user
            var user = new User { Id = userId };

            // Get user's roles
            var userRoles = await _identityBroker.GetUserRolesAsync(user);

            // Prevent removing last role
            if (userRoles.Count == 1)
                throw new InvalidOperationException("Cannot remove the last role from a user");

            // Prevent removing last Admin role (business rule)
            if (roleName == "Admin" && userRoles.Count(r => r == "Admin") == 1)
                throw new InvalidOperationException("Cannot remove the last Admin role from the system");

            // Remove role
            var result = await _identityBroker.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to remove role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _roleBroker.GetAllRolesAsync();
        }
    }
}
