using AutoNext.Platform.AccessControl.API.Models.Entities;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public interface IPermissionRepository : IRepository<Permission>
    {
        Task<Permission?> GetByCodeAsync(string code);
        Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(Guid roleId);
        Task<IEnumerable<Permission>> GetPermissionsByUserAsync(Guid userId);
        Task<IEnumerable<Permission>> GetPermissionsByResourceAsync(string resource);
        Task<IEnumerable<string>> GetPermissionCodesByUserAsync(Guid userId);
    }
}
