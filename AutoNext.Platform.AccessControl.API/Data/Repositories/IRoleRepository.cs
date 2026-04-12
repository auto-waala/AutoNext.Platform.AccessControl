using AutoNext.Platform.AccessControl.API.Models.Entities;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<Role?> GetByCodeAsync(string code);
        Task<Role?> GetRoleWithPermissionsAsync(Guid roleId);
        Task<IEnumerable<Role>> GetRolesByUserAsync(Guid userId);
        Task<IEnumerable<Role>> GetSystemRolesAsync();
        Task<bool> IsCodeUniqueAsync(string code, Guid? excludeRoleId = null);
    }
}
