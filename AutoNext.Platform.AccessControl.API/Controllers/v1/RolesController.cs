using Asp.Versioning;
using AutoNext.Platform.AccessControl.API.Managers.Interfaces;
using AutoNext.Platform.AccessControl.API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNext.Platform.AccessControl.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IRoleService roleService, ILogger<RolesController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(ApiResponse<IEnumerable<RoleResponseDto>>.Ok(roles, "Roles retrieved successfully"));
        }

        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActive()
        {
            var roles = await _roleService.GetActiveRolesAsync();
            return Ok(ApiResponse<IEnumerable<RoleResponseDto>>.Ok(roles, "Active roles retrieved successfully"));
        }

        [HttpGet("{roleId:Guid}")]
        [ProducesResponseType(typeof(ApiResponse<RoleResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid roleId)
        {
            var role = await _roleService.GetRoleByIdAsync(roleId);
            if (role == null)
                return NotFound(ApiResponse<object>.NotFound($"Role with ID {roleId} not found"));

            return Ok(ApiResponse<RoleResponseDto>.Ok(role, "Role retrieved successfully"));
        }

        [HttpGet("code/{code}")]
        [ProducesResponseType(typeof(ApiResponse<RoleResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCode(string code)
        {
            var role = await _roleService.GetRoleByCodeAsync(code);
            if (role == null)
                return NotFound(ApiResponse<object>.NotFound($"Role with code {code} not found"));

            return Ok(ApiResponse<RoleResponseDto>.Ok(role, "Role retrieved successfully"));
        }


        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RoleResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] RoleCreateDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid request", 400));

            try
            {
                var role = await _roleService.CreateRoleAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { roleId = role.Id },
                    ApiResponse<RoleResponseDto>.Ok(role, "Role created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }

        [HttpPut("{roleId:Guid}")]
        [ProducesResponseType(typeof(ApiResponse<RoleResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid roleId, [FromBody] RoleUpdateDto updateDto)
        {
            try
            {
                var role = await _roleService.UpdateRoleAsync(roleId, updateDto);
                if (role == null)
                    return NotFound(ApiResponse<object>.NotFound($"Role with ID {roleId} not found"));

                return Ok(ApiResponse<RoleResponseDto>.Ok(role, "Role updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }

        [HttpPatch("{roleId:Guid}/toggle/{isActive}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleStatus(Guid roleId, bool isActive)
        {
            var result = await _roleService.ToggleRoleStatusAsync(roleId, isActive);
            if (!result)
                return NotFound(ApiResponse<object>.NotFound($"Role with ID {roleId} not found"));

            return Ok(ApiResponse<object>.Ok(null, $"Role status changed to {(isActive ? "active" : "inactive")}"));
        }

        [HttpPost("{roleId:Guid}/permissions")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignPermissions(Guid roleId, [FromBody] List<Guid> permissionIds)
        {
            await _roleService.AssignPermissionsAsync(roleId, permissionIds);
            return Ok(ApiResponse<object>.Ok(null, "Permissions assigned successfully"));
        }

        [HttpDelete("{roleId:Guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid roleId)
        {
            try
            {
                var deleted = await _roleService.DeleteRoleAsync(roleId);
                if (!deleted)
                    return NotFound(ApiResponse<object>.NotFound($"Role with ID {roleId} not found"));

                return Ok(ApiResponse<object>.Ok(null, "Role deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }
    }
}