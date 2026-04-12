using AutoNext.Platform.AccessControl.API.Models.Entities;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public interface IUserRoleRepository : IRepository<UserRole>
    {
        Task AddUserRoleAsync(UserRole userRole);
        Task<IEnumerable<UserRole>> GetUserRolesByUserAsync(Guid userId);
        Task<IEnumerable<UserRole>> GetUserRolesByRoleAsync(Guid roleId);
        Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId, Guid? organizationId = null);
        Task RemoveUserRolesAsync(Guid userId);
        Task RemoveUserRoleAsync(Guid userId, Guid roleId, Guid? organizationId = null);
    }
}
