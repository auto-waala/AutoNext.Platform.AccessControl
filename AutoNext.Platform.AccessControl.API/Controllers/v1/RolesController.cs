using Asp.Versioning;
using AutoNext.Platform.AccessControl.API.Managers.Interfaces;
using AutoNext.Platform.AccessControl.API.Models.DTOs;
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
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Fetching all roles");

            var roles = await _roleService.GetAllRolesAsync();

            _logger.LogInformation("Retrieved {Count} roles", roles.Count());

            return Ok(ApiResponse<IEnumerable<RoleResponseDto>>.Ok(roles));
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            _logger.LogInformation("Fetching active roles");

            var roles = await _roleService.GetActiveRolesAsync();

            return Ok(ApiResponse<IEnumerable<RoleResponseDto>>.Ok(roles));
        }

        [HttpGet("{roleId:Guid}")]
        public async Task<IActionResult> GetById(Guid roleId)
        {
            _logger.LogInformation("Fetching role: {RoleId}", roleId);

            var role = await _roleService.GetRoleByIdAsync(roleId);

            if (role == null)
            {
                _logger.LogWarning("Role not found: {RoleId}", roleId);
                return NotFound(ApiResponse<object>.NotFound($"Role {roleId} not found"));
            }

            return Ok(ApiResponse<RoleResponseDto>.Ok(role));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoleCreateDto dto)
        {
            _logger.LogInformation("Creating role: {Code}", dto.Code);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid role create request");
                return BadRequest(ApiResponse<object>.Error("Invalid request"));
            }

            try
            {
                var role = await _roleService.CreateRoleAsync(dto);

                _logger.LogInformation("Role created: {RoleId}", role.Id);

                return CreatedAtAction(nameof(GetById),
                    new { roleId = role.Id },
                    ApiResponse<RoleResponseDto>.Ok(role));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error creating role");
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpPut("{roleId:Guid}")]
        public async Task<IActionResult> Update(Guid roleId, [FromBody] RoleUpdateDto dto)
        {
            _logger.LogInformation("Updating role: {RoleId}", roleId);

            try
            {
                var role = await _roleService.UpdateRoleAsync(roleId, dto);

                if (role == null)
                {
                    _logger.LogWarning("Role not found: {RoleId}", roleId);
                    return NotFound(ApiResponse<object>.NotFound("Role not found"));
                }

                return Ok(ApiResponse<RoleResponseDto>.Ok(role));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error updating role: {RoleId}", roleId);
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpDelete("{roleId:Guid}")]
        public async Task<IActionResult> Delete(Guid roleId)
        {
            _logger.LogInformation("Deleting role: {RoleId}", roleId);

            var deleted = await _roleService.DeleteRoleAsync(roleId);

            if (!deleted)
            {
                _logger.LogWarning("Role not found: {RoleId}", roleId);
                return NotFound(ApiResponse<object>.NotFound("Role not found"));
            }

            return Ok(ApiResponse<object>.Ok(null, "Deleted successfully"));
        }
    }
}