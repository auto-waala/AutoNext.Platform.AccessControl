using AutoNext.Platform.AccessControl.API.Models.Entities;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public interface IRolePermissionRepository : IRepository<RolePermission>
    {
        Task<IEnumerable<RolePermission>> GetRolePermissionsByRoleAsync(Guid roleId);
        Task<IEnumerable<RolePermission>> GetRolePermissionsByPermissionAsync(Guid permissionId);
        Task<RolePermission?> GetRolePermissionAsync(Guid roleId, Guid permissionId);
        Task RemoveRolePermissionsAsync(Guid roleId);
        Task RemoveRolePermissionAsync(Guid roleId, Guid permissionId);
    }
}
