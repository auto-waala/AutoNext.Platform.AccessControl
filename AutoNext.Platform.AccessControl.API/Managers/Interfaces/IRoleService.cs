using AutoNext.Platform.AccessControl.API.Models.DTOs;

namespace AutoNext.Platform.AccessControl.API.Managers.Interfaces
{
    public interface IRoleService
    {
        Task<RoleResponseDto?> GetRoleByIdAsync(Guid roleId);
        Task<RoleResponseDto?> GetRoleByCodeAsync(string code);
        Task<IEnumerable<RoleResponseDto>> GetAllRolesAsync();
        Task<IEnumerable<RoleResponseDto>> GetActiveRolesAsync();
        Task<RoleResponseDto> CreateRoleAsync(RoleCreateDto createDto);
        Task<RoleResponseDto?> UpdateRoleAsync(Guid roleId, RoleUpdateDto updateDto);
        Task<bool> DeleteRoleAsync(Guid roleId);
        Task<bool> ToggleRoleStatusAsync(Guid roleId, bool isActive);
        Task<bool> AssignPermissionsAsync(Guid roleId, List<Guid> permissionIds);
    }
}
