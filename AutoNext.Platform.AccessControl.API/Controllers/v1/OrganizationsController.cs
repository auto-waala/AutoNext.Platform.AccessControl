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
    [Authorize]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly ILogger<OrganizationsController> _logger;

        public OrganizationsController(IOrganizationService organizationService, ILogger<OrganizationsController> logger)
        {
            _organizationService = organizationService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = "Organizations.Read")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrganizationResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var organizations = await _organizationService.GetAllOrganizationsAsync();
            return Ok(ApiResponse<IEnumerable<OrganizationResponseDto>>.Ok(organizations, "Organizations retrieved successfully"));
        }

        [HttpGet("{organizationId:Guid}")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid organizationId)
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(organizationId);
            if (organization == null)
                return NotFound(ApiResponse<object>.NotFound($"Organization with ID {organizationId} not found"));

            return Ok(ApiResponse<OrganizationResponseDto>.Ok(organization, "Organization retrieved successfully"));
        }

        [HttpGet("code/{code}")]
        [Authorize(Policy = "Organizations.Read")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCode(string code)
        {
            var organization = await _organizationService.GetOrganizationByCodeAsync(code);
            if (organization == null)
                return NotFound(ApiResponse<object>.NotFound($"Organization with code {code} not found"));

            return Ok(ApiResponse<OrganizationResponseDto>.Ok(organization, "Organization retrieved successfully"));
        }

        [HttpGet("type/{organizationType}")]
        [Authorize(Policy = "Organizations.Read")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrganizationResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByType(string organizationType)
        {
            var organizations = await _organizationService.GetOrganizationsByTypeAsync(organizationType);
            return Ok(ApiResponse<IEnumerable<OrganizationResponseDto>>.Ok(organizations, "Organizations retrieved successfully"));
        }

        
        [HttpPost]
        [Authorize(Policy = "Organizations.Write")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] OrganizationCreateDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid request", 400));

            try
            {
                var organization = await _organizationService.CreateOrganizationAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { organizationId = organization.Id },
                    ApiResponse<OrganizationResponseDto>.Ok(organization, "Organization created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }

        [HttpPut("{organizationId:Guid}")]
        [Authorize(Policy = "Organizations.Write")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid organizationId, [FromBody] OrganizationUpdateDto updateDto)
        {
            try
            {
                var organization = await _organizationService.UpdateOrganizationAsync(organizationId, updateDto);
                if (organization == null)
                    return NotFound(ApiResponse<object>.NotFound($"Organization with ID {organizationId} not found"));

                return Ok(ApiResponse<OrganizationResponseDto>.Ok(organization, "Organization updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }

        [HttpPatch("{organizationId:Guid}/toggle/{isActive}")]
        [Authorize(Policy = "Organizations.Write")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleStatus(Guid organizationId, bool isActive)
        {
            var result = await _organizationService.ToggleOrganizationStatusAsync(organizationId, isActive);
            if (!result)
                return NotFound(ApiResponse<object>.NotFound($"Organization with ID {organizationId} not found"));

            return Ok(ApiResponse<object>.Ok(null, $"Organization status changed to {(isActive ? "active" : "inactive")}"));
        }

        [HttpPost("{organizationId:Guid}/users/{userId:Guid}")]
        [Authorize(Policy = "Organizations.Write")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddUser(Guid organizationId, Guid userId, [FromQuery] string role = "Member")
        {
            await _organizationService.AddUserToOrganizationAsync(userId, organizationId, role);
            return Ok(ApiResponse<object>.Ok(null, "User added to organization successfully"));
        }

        [HttpDelete("{organizationId:Guid}/users/{userId:Guid}")]
        [Authorize(Policy = "Organizations.Write")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveUser(Guid organizationId, Guid userId)
        {
            await _organizationService.RemoveUserFromOrganizationAsync(userId, organizationId);
            return Ok(ApiResponse<object>.Ok(null, "User removed from organization successfully"));
        }

        [HttpDelete("{organizationId:Guid}")]
        [Authorize(Policy = "Organizations.Delete")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid organizationId)
        {
            try
            {
                var deleted = await _organizationService.DeleteOrganizationAsync(organizationId);
                if (!deleted)
                    return NotFound(ApiResponse<object>.NotFound($"Organization with ID {organizationId} not found"));

                return Ok(ApiResponse<object>.Ok(null, "Organization deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }
    }
}
