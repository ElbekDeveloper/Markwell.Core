using Markwell.Core.Brokers;
using Markwell.Core.Entities;
using Markwell.Core.Models;

namespace Markwell.Core.Services
{
    public class UserService
    {
        private readonly UserBroker _userBroker;
        private readonly IdentityBroker _identityBroker;
        private readonly RoleBroker _roleBroker;
        private readonly PasswordValidationService _passwordValidationService;

        public UserService(
            UserBroker userBroker,
            IdentityBroker identityBroker,
            RoleBroker roleBroker,
            PasswordValidationService passwordValidationService)
        {
            _userBroker = userBroker;
            _identityBroker = identityBroker;
            _roleBroker = roleBroker;
            _passwordValidationService = passwordValidationService;
        }

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, string createdByUserId)
        {
            // Validate email format
            if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
                throw new ArgumentException("Invalid email format", nameof(request.Email));

            // Validate full name
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new ArgumentException("Full name is required", nameof(request.FullName));

            // Check email uniqueness
            var existingUser = await _userBroker.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
                throw new InvalidOperationException($"Email '{request.Email}' already exists");

            // Create user without password
            var user = new User
            {
                Email = request.Email,
                UserName = request.Email,
                FullName = request.FullName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _identityBroker.CreateUserAsync(user, "TempPassword@123");
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            // Assign role
            var role = await _roleBroker.GetRoleByNameAsync(request.RoleName);
            if (role == null)
                throw new ArgumentException($"Role '{request.RoleName}' does not exist", nameof(request.RoleName));

            var roleResult = await _identityBroker.AddToRoleAsync(user, request.RoleName);
            if (!roleResult.Succeeded)
                throw new InvalidOperationException($"Failed to assign role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");

            // Retrieve updated user with roles
            var createdUser = await _userBroker.GetUserByIdAsync(user.Id);
            if (createdUser == null)
                throw new InvalidOperationException("Failed to retrieve created user");

            var userRoles = await _identityBroker.GetUserRolesAsync(createdUser);

            return MapToUserResponse(createdUser, userRoles.ToList());
        }

        public async Task<UserResponse> GetUserAsync(string userId)
        {
            var user = await _userBroker.GetUserByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User '{userId}' not found");

            var roles = await _identityBroker.GetUserRolesAsync(user);
            return MapToUserResponse(user, roles.ToList());
        }

        private UserResponse MapToUserResponse(User user, List<string> roles)
        {
            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                FullName = user.FullName,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive,
                Roles = roles,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
