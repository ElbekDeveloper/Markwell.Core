using Markwell.Core.Brokers;
using Markwell.Core.Entities;
using Markwell.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace Markwell.Core.Services
{
    public class AuthenticationService
    {
        private readonly IdentityBroker _identityBroker;
        private readonly UserBroker _userBroker;
        private readonly PasswordValidationService _passwordValidationService;
        private readonly EmailVerificationService _emailVerificationService;

        public AuthenticationService(
            IdentityBroker identityBroker,
            UserBroker userBroker,
            PasswordValidationService passwordValidationService,
            EmailVerificationService emailVerificationService)
        {
            _identityBroker = identityBroker;
            _userBroker = userBroker;
            _passwordValidationService = passwordValidationService;
            _emailVerificationService = emailVerificationService;
        }

        public async Task<UserResponse> RegisterAsync(RegisterRequest request)
        {
            // Validate email format
            if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
                throw new ArgumentException("Invalid email format", nameof(request.Email));

            // Validate password strength
            if (!_passwordValidationService.ValidatePassword(request.Password))
            {
                var message = _passwordValidationService.GetPasswordStrengthMessage(request.Password);
                throw new ArgumentException(message, nameof(request.Password));
            }

            // Validate full name
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new ArgumentException("Full name is required", nameof(request.FullName));

            // Check email uniqueness
            var existingUser = await _userBroker.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
                throw new InvalidOperationException($"Email '{request.Email}' is already registered");

            // Create user
            var user = new User
            {
                Email = request.Email,
                UserName = request.Email,
                FullName = request.FullName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _identityBroker.CreateUserAsync(user, request.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Registration failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            // Assign Student role by default
            await _identityBroker.AddToRoleAsync(user, "Student");

            // Generate email verification token
            var verificationToken = await _emailVerificationService.GenerateVerificationToken(user);

            // Retrieve created user
            var createdUser = await _userBroker.GetUserByIdAsync(user.Id);
            if (createdUser == null)
                throw new InvalidOperationException("Failed to retrieve created user");

            var roles = await _identityBroker.GetUserRolesAsync(createdUser);

            return new UserResponse
            {
                Id = createdUser.Id,
                Email = createdUser.Email ?? string.Empty,
                UserName = createdUser.UserName ?? string.Empty,
                FullName = createdUser.FullName,
                EmailConfirmed = false, // Always false initially
                IsActive = createdUser.IsActive,
                Roles = roles.ToList(),
                CreatedAt = createdUser.CreatedAt,
                UpdatedAt = createdUser.UpdatedAt,
                LastLoginAt = createdUser.LastLoginAt
            };
        }

        public async Task<(UserResponse, string? token)> LoginAsync(LoginRequest request)
        {
            // Validate email format
            if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
                throw new ArgumentException("Invalid email format", nameof(request.Email));

            // Validate password provided
            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Password is required", nameof(request.Password));

            // Find user by email
            var user = await _userBroker.GetUserByEmailAsync(request.Email);
            if (user == null)
                throw new InvalidOperationException("Invalid email or password");

            // Check if account is active
            if (!user.IsActive)
                throw new InvalidOperationException("Account is disabled");

            // Check if email is confirmed
            if (!user.EmailConfirmed)
                throw new InvalidOperationException("Email not confirmed. Please verify your email before logging in");

            // Verify password
            var signInResult = await _identityBroker.SignInAsync(user, request.Password);
            if (!signInResult.Succeeded)
                throw new InvalidOperationException("Invalid email or password");

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userBroker.UpdateUserAsync(user);

            // Get user roles
            var roles = await _identityBroker.GetUserRolesAsync(user);

            var userResponse = new UserResponse
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                FullName = user.FullName,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt
            };

            // Generate JWT token (simplified - use external library in production)
            var token = GenerateJwtToken(user, roles.ToList());

            return (userResponse, token);
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userBroker.GetUserByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User '{userId}' not found");

            var result = await _emailVerificationService.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Email confirmation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            // Update user's email confirmed flag
            user.EmailConfirmed = true;
            await _userBroker.UpdateUserAsync(user);

            return true;
        }

        private string GenerateJwtToken(User user, List<string> roles)
        {
            // Simplified token generation - in production use a proper JWT library
            var tokenData = new
            {
                userId = user.Id,
                email = user.Email,
                roles = roles,
                issuedAt = DateTime.UtcNow,
                expiresAt = DateTime.UtcNow.AddHours(1)
            };

            return System.Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(
                    System.Text.Json.JsonSerializer.Serialize(tokenData)
                )
            );
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
