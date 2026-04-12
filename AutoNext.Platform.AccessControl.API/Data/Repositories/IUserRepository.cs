using AutoNext.Platform.AccessControl.API.Models.Entities;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetUserWithRolesAsync(Guid userId);
        Task<User?> GetUserWithPermissionsAsync(Guid userId);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string roleCode);
        Task<IEnumerable<User>> GetUsersByOrganizationAsync(Guid organizationId);
        Task<IEnumerable<User>> GetUsersByTypeAsync(string userType);
        Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null);
    }
}
