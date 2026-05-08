using Asp.Versioning;
using AutoNext.Platform.AccessControl.API.Managers.Interfaces;
using AutoNext.Platform.AccessControl.API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoNext.Platform.AccessControl.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("{userId:Guid}")]
        public async Task<IActionResult> GetById(Guid userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("admin") || User.IsInRole("super_admin");

            _logger.LogInformation("Fetching user {UserId} by {CurrentUser}", userId, currentUserId);

            if (!isAdmin && currentUserId != userId.ToString())
            {
                _logger.LogWarning("Unauthorized access attempt by {UserId}", currentUserId);
                return Forbid();
            }

            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return NotFound(ApiResponse<object>.NotFound("User not found"));
            }

            return Ok(ApiResponse<UserResponseDto>.Ok(user));
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation("Fetching current user {UserId}", userId);

            if (userId == null)
                return Unauthorized(ApiResponse<object>.Unauthorized());

            var user = await _userService.GetUserByIdAsync(Guid.Parse(userId));

            return Ok(ApiResponse<UserResponseDto>.Ok(user!));
        }

        [HttpPost]
        [Authorize(Policy = "Users.Write")]
        public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
        {
            _logger.LogInformation("Creating user: {Email}", dto.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid user create request");
                return BadRequest(ApiResponse<object>.Error("Invalid request"));
            }

            try
            {
                var user = await _userService.CreateUserAsync(dto);

                _logger.LogInformation("User created: {UserId}", user.Id);

                return CreatedAtAction(nameof(GetById),
                    new { userId = user.Id },
                    ApiResponse<UserResponseDto>.Ok(user));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error creating user");
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpPut("{userId:Guid}")]
        public async Task<IActionResult> Update(Guid userId, [FromBody] UserUpdateDto dto)
        {
            _logger.LogInformation("Updating user: {UserId}", userId);

            try
            {
                var user = await _userService.UpdateUserAsync(userId, dto);

                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return NotFound(ApiResponse<object>.NotFound("User not found"));
                }

                return Ok(ApiResponse<UserResponseDto>.Ok(user));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error updating user");
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpDelete("{userId:Guid}")]
        [Authorize(Policy = "Users.Delete")]
        public async Task<IActionResult> Delete(Guid userId)
        {
            _logger.LogInformation("Deleting user: {UserId}", userId);

            var deleted = await _userService.DeleteUserAsync(userId);

            if (!deleted)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return NotFound(ApiResponse<object>.NotFound("User not found"));
            }

            return Ok(ApiResponse<object>.Ok(null, "Deleted successfully"));
        }
    }
}