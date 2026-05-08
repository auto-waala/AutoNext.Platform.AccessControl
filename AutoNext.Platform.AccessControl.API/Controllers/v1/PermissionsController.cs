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
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(IPermissionService permissionService, ILogger<PermissionsController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Fetching all permissions");

            var permissions = await _permissionService.GetAllPermissionsAsync();

            _logger.LogInformation("Retrieved {Count} permissions", permissions.Count());

            return Ok(ApiResponse<IEnumerable<PermissionDto>>.Ok(permissions, "Permissions retrieved successfully"));
        }

        [HttpGet("{permissionId:Guid}")]
        public async Task<IActionResult> GetById(Guid permissionId)
        {
            _logger.LogInformation("Fetching permission by Id: {PermissionId}", permissionId);

            var permission = await _permissionService.GetPermissionByIdAsync(permissionId);

            if (permission == null)
            {
                _logger.LogWarning("Permission not found: {PermissionId}", permissionId);
                return NotFound(ApiResponse<object>.NotFound($"Permission with ID {permissionId} not found"));
            }

            return Ok(ApiResponse<PermissionDto>.Ok(permission, "Permission retrieved successfully"));
        }

        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            _logger.LogInformation("Fetching permission by Code: {Code}", code);

            var permission = await _permissionService.GetPermissionByCodeAsync(code);

            if (permission == null)
            {
                _logger.LogWarning("Permission not found with Code: {Code}", code);
                return NotFound(ApiResponse<object>.NotFound($"Permission with code {code} not found"));
            }

            return Ok(ApiResponse<PermissionDto>.Ok(permission, "Permission retrieved successfully"));
        }

        [HttpGet("resource/{resource}")]
        public async Task<IActionResult> GetByResource(string resource)
        {
            _logger.LogInformation("Fetching permissions by Resource: {Resource}", resource);

            var permissions = await _permissionService.GetPermissionsByResourceAsync(resource);

            return Ok(ApiResponse<IEnumerable<PermissionDto>>.Ok(permissions, "Permissions retrieved successfully"));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PermissionCreateDto createDto)
        {
            _logger.LogInformation("Creating permission: {Code}", createDto.Code);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid create permission request");
                return BadRequest(ApiResponse<object>.Error("Invalid request", 400));
            }

            try
            {
                var permission = await _permissionService.CreatePermissionAsync(createDto);

                _logger.LogInformation("Permission created successfully: {PermissionId}", permission.Id);

                return CreatedAtAction(nameof(GetById),
                    new { permissionId = permission.Id },
                    ApiResponse<PermissionDto>.Ok(permission, "Permission created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error creating permission");
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }

        [HttpDelete("{permissionId:Guid}")]
        public async Task<IActionResult> Delete(Guid permissionId)
        {
            _logger.LogInformation("Deleting permission: {PermissionId}", permissionId);

            try
            {
                var deleted = await _permissionService.DeletePermissionAsync(permissionId);

                if (!deleted)
                {
                    _logger.LogWarning("Permission not found for deletion: {PermissionId}", permissionId);
                    return NotFound(ApiResponse<object>.NotFound($"Permission with ID {permissionId} not found"));
                }

                _logger.LogInformation("Permission deleted successfully: {PermissionId}", permissionId);

                return Ok(ApiResponse<object>.Ok(null, "Permission deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error deleting permission: {PermissionId}", permissionId);
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }

        [HttpGet("user/{userId:Guid}")]
        [Authorize(Policy = "Users.Read")]
        public async Task<IActionResult> GetUserPermissions(Guid userId)
        {
            _logger.LogInformation("Fetching permissions for user: {UserId}", userId);

            var permissions = await _permissionService.GetUserPermissionsAsync(userId);

            return Ok(ApiResponse<IEnumerable<string>>.Ok(permissions, "User permissions retrieved successfully"));
        }

        [HttpGet("check")]
        [Authorize]
        public async Task<IActionResult> HasPermission([FromQuery] string permissionCode)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                _logger.LogWarning("Unauthorized permission check attempt");
                return Unauthorized(ApiResponse<object>.Unauthorized());
            }

            _logger.LogInformation("Checking permission {PermissionCode} for user {UserId}", permissionCode, userId);

            var hasPermission = await _permissionService.UserHasPermissionAsync(Guid.Parse(userId), permissionCode);

            return Ok(ApiResponse<bool>.Ok(hasPermission, "Permission check completed"));
        }
    }
}