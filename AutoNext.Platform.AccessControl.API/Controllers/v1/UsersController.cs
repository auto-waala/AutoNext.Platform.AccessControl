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
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("admin") || User.IsInRole("super_admin");

            if (!isAdmin && currentUserId != userId.ToString())
                return Forbid();

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<object>.NotFound($"User with ID {userId} not found"));

            return Ok(ApiResponse<UserResponseDto>.Ok(user, "User retrieved successfully"));
        }

        [HttpGet("email/{email}")]
        [Authorize(Policy = "Users.Read")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound(ApiResponse<object>.NotFound($"User with email {email} not found"));

            return Ok(ApiResponse<UserResponseDto>.Ok(user, "User retrieved successfully"));
        }

        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(ApiResponse<object>.Unauthorized());

            var user = await _userService.GetUserByIdAsync(Guid.Parse(userId));
            return Ok(ApiResponse<UserResponseDto>.Ok(user!, "Current user retrieved successfully"));
        }

        [HttpGet("type/{userType}")]
        [Authorize(Policy = "Users.Read")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByType(string userType)
        {
            var users = await _userService.GetUsersByTypeAsync(userType);
            return Ok(ApiResponse<IEnumerable<UserResponseDto>>.Ok(users, "Users retrieved successfully"));
        }

        [HttpGet("organization/{organizationId:Guid}")]
        [Authorize(Policy = "Users.Read")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByOrganization(Guid organizationId)
        {
            var users = await _userService.GetUsersByOrganizationAsync(organizationId);
            return Ok(ApiResponse<IEnumerable<UserResponseDto>>.Ok(users, "Users retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Policy = "Users.Write")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] UserCreateDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid request", 400));

            try
            {
                var user = await _userService.CreateUserAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { userId = user.Id },
                    ApiResponse<UserResponseDto>.Ok(user, "User created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }

        [HttpPut("{userId:Guid}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid userId, [FromBody] UserUpdateDto updateDto)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("admin") || User.IsInRole("super_admin");

            if (!isAdmin && currentUserId != userId.ToString())
                return Forbid();

            try
            {
                var user = await _userService.UpdateUserAsync(userId, updateDto);
                if (user == null)
                    return NotFound(ApiResponse<object>.NotFound($"User with ID {userId} not found"));

                return Ok(ApiResponse<UserResponseDto>.Ok(user, "User updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }

        [HttpPut("me")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UserUpdateDto updateDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(ApiResponse<object>.Unauthorized());

            var user = await _userService.UpdateUserAsync(Guid.Parse(userId), updateDto);
            return Ok(ApiResponse<UserResponseDto>.Ok(user!, "Profile updated successfully"));
        }

        [HttpPatch("{userId:Guid}/toggle/{isActive}")]
        [Authorize(Policy = "Users.Write")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleStatus(Guid userId, bool isActive)
        {
            var result = await _userService.ToggleUserStatusAsync(userId, isActive);
            if (!result)
                return NotFound(ApiResponse<object>.NotFound($"User with ID {userId} not found"));

            return Ok(ApiResponse<object>.Ok(null, $"User status changed to {(isActive ? "active" : "inactive")}"));
        }

        [HttpPost("{userId:Guid}/roles")]
        [Authorize(Policy = "Users.Write")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignRoles(Guid userId, [FromBody] List<Guid> roleIds)
        {
            await _userService.AssignRolesAsync(userId, roleIds);
            return Ok(ApiResponse<object>.Ok(null, "Roles assigned successfully"));
        }

        [HttpPost("{userId:Guid}/organizations/{organizationId:Guid}")]
        [Authorize(Policy = "Users.Write")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignOrganization(Guid userId, Guid organizationId, [FromQuery] bool isPrimary = false)
        {
            await _userService.AssignOrganizationAsync(userId, organizationId, isPrimary);
            return Ok(ApiResponse<object>.Ok(null, "Organization assigned successfully"));
        }

        [HttpDelete("{userId:Guid}")]
        [Authorize(Policy = "Users.Delete")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid userId)
        {
            var deleted = await _userService.DeleteUserAsync(userId);
            if (!deleted)
                return NotFound(ApiResponse<object>.NotFound($"User with ID {userId} not found"));

            return Ok(ApiResponse<object>.Ok(null, "User deleted successfully"));
        }
    }
}