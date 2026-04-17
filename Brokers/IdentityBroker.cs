using Markwell.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Markwell.Core.Brokers
{
    public class IdentityBroker
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;

        public IdentityBroker(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<IdentityResult> CreateUserAsync(User user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            return result;
        }

        public async Task<User?> FindUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user;
        }

        public async Task<IdentityResult> UpdateUserAsync(User user)
        {
            var result = await _userManager.UpdateAsync(user);
            return result;
        }

        public async Task<IdentityResult> DeleteUserAsync(User user)
        {
            var result = await _userManager.DeleteAsync(user);
            return result;
        }

        public async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
        {
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result;
        }

        public async Task<SignInResult> SignInAsync(User user, string password)
        {
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            return result;
        }

        public async Task<IList<string>> GetUserRolesAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles;
        }

        public async Task<IdentityResult> AddToRoleAsync(User user, string roleName)
        {
            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result;
        }

        public async Task<IdentityResult> RemoveFromRoleAsync(User user, string roleName)
        {
            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            return result;
        }
    }
}
