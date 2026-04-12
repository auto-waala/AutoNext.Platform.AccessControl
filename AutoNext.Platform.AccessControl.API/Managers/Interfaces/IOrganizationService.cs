using AutoNext.Platform.AccessControl.API.Models.DTOs;

namespace AutoNext.Platform.AccessControl.API.Managers.Interfaces
{
    public interface IOrganizationService
    {
        Task<OrganizationResponseDto?> GetOrganizationByIdAsync(Guid organizationId);
        Task<OrganizationResponseDto?> GetOrganizationByCodeAsync(string code);
        Task<IEnumerable<OrganizationResponseDto>> GetAllOrganizationsAsync();
        Task<IEnumerable<OrganizationResponseDto>> GetOrganizationsByTypeAsync(string organizationType);
        Task<OrganizationResponseDto> CreateOrganizationAsync(OrganizationCreateDto createDto);
        Task<OrganizationResponseDto?> UpdateOrganizationAsync(Guid organizationId, OrganizationUpdateDto updateDto);
        Task<bool> DeleteOrganizationAsync(Guid organizationId);
        Task<bool> ToggleOrganizationStatusAsync(Guid organizationId, bool isActive);
        Task<bool> AddUserToOrganizationAsync(Guid userId, Guid organizationId, string role = "Member");
        Task<bool> RemoveUserFromOrganizationAsync(Guid userId, Guid organizationId);
    }
}
