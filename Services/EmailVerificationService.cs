using Markwell.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Markwell.Core.Services
{
    public class EmailVerificationService
    {
        private readonly UserManager<User> _userManager;

        public EmailVerificationService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string> GenerateVerificationToken(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return token;
        }

        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result;
        }
    }
}
