using Markwell.Core.Models;
using Markwell.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Markwell.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly ProfileManagementOrchestrationService _orchestrationService;

        public AuthController(ProfileManagementOrchestrationService orchestrationService)
        {
            _orchestrationService = orchestrationService;
        }

        /// <summary>
        /// Register a new user account
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _orchestrationService.RegisterUserAsync(request);
                return CreatedAtAction(nameof(Register), new { email = user.Email }, user);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already registered"))
            {
                return Conflict(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var (user, token) = await _orchestrationService.LoginUserAsync(request);
                
                var response = new LoginResponse
                {
                    User = user,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(1)
                };

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Email not confirmed"))
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("disabled"))
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (InvalidOperationException)
            {
                return Unauthorized(new { error = "Invalid email or password" });
            }
        }

        /// <summary>
        /// Confirm email address with verification token
        /// </summary>
        [HttpPost("confirm-email")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
        {
            try
            {
                await _orchestrationService.ConfirmEmailAsync(request.UserId, request.Token);
                return Ok(new { message = "Email confirmed successfully. You can now login." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class LoginResponse
    {
        public UserResponse User { get; set; } = null!;
        public string? Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class ConfirmEmailRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
