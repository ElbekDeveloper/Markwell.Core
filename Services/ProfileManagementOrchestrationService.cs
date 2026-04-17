using Markwell.Core.Models;
using Markwell.Core.Entities;

namespace Markwell.Core.Services
{
    /// <summary>
    /// Orchestration service that coordinates user management, authentication, and role operations.
    /// Provides a unified interface for profile management features.
    /// </summary>
    public class ProfileManagementOrchestrationService
    {
        private readonly UserService _userService;
        private readonly AuthenticationService _authenticationService;
        private readonly RoleService _roleService;

        public ProfileManagementOrchestrationService(
            UserService userService,
            AuthenticationService authenticationService,
            RoleService roleService)
        {
            _userService = userService;
            _authenticationService = authenticationService;
            _roleService = roleService;
        }

        // User Management Operations
        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, string createdByUserId)
        {
            return await _userService.CreateUserAsync(request, createdByUserId);
        }

        public async Task<UserResponse> GetUserAsync(string userId)
        {
            return await _userService.GetUserAsync(userId);
        }

        // Authentication Operations
        public async Task<UserResponse> RegisterUserAsync(RegisterRequest request)
        {
            return await _authenticationService.RegisterAsync(request);
        }

        public async Task<(UserResponse, string? token)> LoginUserAsync(LoginRequest request)
        {
            return await _authenticationService.LoginAsync(request);
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            return await _authenticationService.ConfirmEmailAsync(userId, token);
        }

        // Role Management Operations
        public async Task AssignRoleAsync(string userId, string roleName, string assignedByUserId)
        {
            await _roleService.AssignRoleAsync(userId, roleName, assignedByUserId);
        }

        public async Task RemoveRoleAsync(string userId, string roleName, string removedByUserId)
        {
            await _roleService.RemoveRoleAsync(userId, roleName, removedByUserId);
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _roleService.GetAllRolesAsync();
        }
    }
}
