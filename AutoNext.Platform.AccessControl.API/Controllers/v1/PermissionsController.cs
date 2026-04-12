using Asp.Versioning;
using AutoNext.Platform.AccessControl.API.Managers.Interfaces;
using AutoNext.Platform.AccessControl.API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoNext.Platform.AccessControl.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Policy = "Permissions.Manage")]
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
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PermissionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(ApiResponse<IEnumerable<PermissionDto>>.Ok(permissions, "Permissions retrieved successfully"));
        }

        [HttpGet("{permissionId:Guid}")]
        [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid permissionId)
        {
            var permission = await _permissionService.GetPermissionByIdAsync(permissionId);
            if (permission == null)
                return NotFound(ApiResponse<object>.NotFound($"Permission with ID {permissionId} not found"));

            return Ok(ApiResponse<PermissionDto>.Ok(permission, "Permission retrieved successfully"));
        }

        [HttpGet("code/{code}")]
        [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCode(string code)
        {
            var permission = await _permissionService.GetPermissionByCodeAsync(code);
            if (permission == null)
                return NotFound(ApiResponse<object>.NotFound($"Permission with code {code} not found"));

            return Ok(ApiResponse<PermissionDto>.Ok(permission, "Permission retrieved successfully"));
        }

        [HttpGet("resource/{resource}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PermissionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByResource(string resource)
        {
            var permissions = await _permissionService.GetPermissionsByResourceAsync(resource);
            return Ok(ApiResponse<IEnumerable<PermissionDto>>.Ok(permissions, "Permissions retrieved successfully"));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] PermissionCreateDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid request", 400));

            try
            {
                var permission = await _permissionService.CreatePermissionAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { permissionId = permission.Id },
                    ApiResponse<PermissionDto>.Ok(permission, "Permission created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }

        [HttpDelete("{permissionId:Guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid permissionId)
        {
            try
            {
                var deleted = await _permissionService.DeletePermissionAsync(permissionId);
                if (!deleted)
                    return NotFound(ApiResponse<object>.NotFound($"Permission with ID {permissionId} not found"));

                return Ok(ApiResponse<object>.Ok(null, "Permission deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }

        [HttpGet("user/{userId:Guid}")]
        [Authorize(Policy = "Users.Read")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserPermissions(Guid userId)
        {
            var permissions = await _permissionService.GetUserPermissionsAsync(userId);
            return Ok(ApiResponse<IEnumerable<string>>.Ok(permissions, "User permissions retrieved successfully"));
        }

        [HttpGet("check")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> HasPermission([FromQuery] string permissionCode)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(ApiResponse<object>.Unauthorized());

            var hasPermission = await _permissionService.UserHasPermissionAsync(Guid.Parse(userId), permissionCode);
            return Ok(ApiResponse<bool>.Ok(hasPermission, "Permission check completed"));
        }
    }
}
