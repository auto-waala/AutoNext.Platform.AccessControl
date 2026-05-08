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
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Fetching all organizations");

            var organizations = await _organizationService.GetAllOrganizationsAsync();

            _logger.LogInformation("Retrieved {Count} organizations", organizations.Count());

            return Ok(ApiResponse<IEnumerable<OrganizationResponseDto>>.Ok(organizations, "Organizations retrieved successfully"));
        }

        [HttpGet("{organizationId:Guid}")]
        public async Task<IActionResult> GetById(Guid organizationId)
        {
            _logger.LogInformation("Fetching organization by Id: {OrganizationId}", organizationId);

            var organization = await _organizationService.GetOrganizationByIdAsync(organizationId);

            if (organization == null)
            {
                _logger.LogWarning("Organization not found: {OrganizationId}", organizationId);
                return NotFound(ApiResponse<object>.NotFound($"Organization with ID {organizationId} not found"));
            }

            return Ok(ApiResponse<OrganizationResponseDto>.Ok(organization, "Organization retrieved successfully"));
        }

        [HttpGet("code/{code}")]
        [Authorize(Policy = "Organizations.Read")]
        public async Task<IActionResult> GetByCode(string code)
        {
            _logger.LogInformation("Fetching organization by Code: {Code}", code);

            var organization = await _organizationService.GetOrganizationByCodeAsync(code);

            if (organization == null)
            {
                _logger.LogWarning("Organization not found with Code: {Code}", code);
                return NotFound(ApiResponse<object>.NotFound($"Organization with code {code} not found"));
            }

            return Ok(ApiResponse<OrganizationResponseDto>.Ok(organization, "Organization retrieved successfully"));
        }

        [HttpGet("type/{organizationType}")]
        [Authorize(Policy = "Organizations.Read")]
        public async Task<IActionResult> GetByType(string organizationType)
        {
            _logger.LogInformation("Fetching organizations by Type: {Type}", organizationType);

            var organizations = await _organizationService.GetOrganizationsByTypeAsync(organizationType);

            return Ok(ApiResponse<IEnumerable<OrganizationResponseDto>>.Ok(organizations, "Organizations retrieved successfully"));
        }

        [HttpPost]
        [Authorize(Policy = "Organizations.Write")]
        public async Task<IActionResult> Create([FromBody] OrganizationCreateDto createDto)
        {
            _logger.LogInformation("Creating organization: {Name}", createDto.Name);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid create organization request");
                return BadRequest(ApiResponse<object>.Error("Invalid request", 400));
            }

            try
            {
                var organization = await _organizationService.CreateOrganizationAsync(createDto);

                _logger.LogInformation("Organization created successfully: {OrganizationId}", organization.Id);

                return CreatedAtAction(nameof(GetById),
                    new { organizationId = organization.Id },
                    ApiResponse<OrganizationResponseDto>.Ok(organization, "Organization created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error creating organization");
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }

        [HttpPut("{organizationId:Guid}")]
        [Authorize(Policy = "Organizations.Write")]
        public async Task<IActionResult> Update(Guid organizationId, [FromBody] OrganizationUpdateDto updateDto)
        {
            _logger.LogInformation("Updating organization: {OrganizationId}", organizationId);

            try
            {
                var organization = await _organizationService.UpdateOrganizationAsync(organizationId, updateDto);

                if (organization == null)
                {
                    _logger.LogWarning("Organization not found for update: {OrganizationId}", organizationId);
                    return NotFound(ApiResponse<object>.NotFound($"Organization with ID {organizationId} not found"));
                }

                _logger.LogInformation("Organization updated successfully: {OrganizationId}", organizationId);

                return Ok(ApiResponse<OrganizationResponseDto>.Ok(organization, "Organization updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error updating organization: {OrganizationId}", organizationId);
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }

        [HttpPatch("{organizationId:Guid}/toggle/{isActive}")]
        [Authorize(Policy = "Organizations.Write")]
        public async Task<IActionResult> ToggleStatus(Guid organizationId, bool isActive)
        {
            _logger.LogInformation("Toggling status for {OrganizationId} to {Status}", organizationId, isActive);

            var result = await _organizationService.ToggleOrganizationStatusAsync(organizationId, isActive);

            if (!result)
            {
                _logger.LogWarning("Organization not found for status toggle: {OrganizationId}", organizationId);
                return NotFound(ApiResponse<object>.NotFound($"Organization with ID {organizationId} not found"));
            }

            return Ok(ApiResponse<object>.Ok(null, $"Organization status changed to {(isActive ? "active" : "inactive")}"));
        }

        [HttpPost("{organizationId:Guid}/users/{userId:Guid}")]
        [Authorize(Policy = "Organizations.Write")]
        public async Task<IActionResult> AddUser(Guid organizationId, Guid userId, [FromQuery] string role = "Member")
        {
            _logger.LogInformation("Adding user {UserId} to organization {OrganizationId} with role {Role}",
                userId, organizationId, role);

            await _organizationService.AddUserToOrganizationAsync(userId, organizationId, role);

            return Ok(ApiResponse<object>.Ok(null, "User added to organization successfully"));
        }

        [HttpDelete("{organizationId:Guid}/users/{userId:Guid}")]
        [Authorize(Policy = "Organizations.Write")]
        public async Task<IActionResult> RemoveUser(Guid organizationId, Guid userId)
        {
            _logger.LogInformation("Removing user {UserId} from organization {OrganizationId}", userId, organizationId);

            await _organizationService.RemoveUserFromOrganizationAsync(userId, organizationId);

            return Ok(ApiResponse<object>.Ok(null, "User removed from organization successfully"));
        }

        [HttpDelete("{organizationId:Guid}")]
        [Authorize(Policy = "Organizations.Delete")]
        public async Task<IActionResult> Delete(Guid organizationId)
        {
            _logger.LogInformation("Deleting organization: {OrganizationId}", organizationId);

            try
            {
                var deleted = await _organizationService.DeleteOrganizationAsync(organizationId);

                if (!deleted)
                {
                    _logger.LogWarning("Organization not found for deletion: {OrganizationId}", organizationId);
                    return NotFound(ApiResponse<object>.NotFound($"Organization with ID {organizationId} not found"));
                }

                _logger.LogInformation("Organization deleted successfully: {OrganizationId}", organizationId);

                return Ok(ApiResponse<object>.Ok(null, "Organization deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error deleting organization: {OrganizationId}", organizationId);
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }
    }
}