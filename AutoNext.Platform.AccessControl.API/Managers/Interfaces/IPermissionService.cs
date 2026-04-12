using AutoNext.Platform.AccessControl.API.Models.DTOs;

namespace AutoNext.Platform.AccessControl.API.Managers.Interfaces
{
    public interface IPermissionService
    {
        Task<PermissionDto?> GetPermissionByIdAsync(Guid permissionId);
        Task<PermissionDto?> GetPermissionByCodeAsync(string code);
        Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
        Task<IEnumerable<PermissionDto>> GetPermissionsByResourceAsync(string resource);
        Task<PermissionDto> CreatePermissionAsync(PermissionCreateDto createDto);
        Task<bool> DeletePermissionAsync(Guid permissionId);
        Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId);
        Task<bool> UserHasPermissionAsync(Guid userId, string permissionCode);
    }
}
