using AutoNext.Platform.AccessControl.API.Models.Entities;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public interface IUserOrganizationRepository : IRepository<UserOrganization>
    {
        Task<IEnumerable<UserOrganization>> GetUserOrganizationsByUserAsync(Guid userId);
        Task<IEnumerable<UserOrganization>> GetUserOrganizationsByOrganizationAsync(Guid organizationId);
        Task<UserOrganization?> GetUserOrganizationAsync(Guid userId, Guid organizationId);
        Task<UserOrganization?> GetPrimaryOrganizationAsync(Guid userId);
        Task RemoveUserFromOrganizationAsync(Guid userId, Guid organizationId);
        Task SetPrimaryOrganizationAsync(Guid userId, Guid organizationId);
    }
}
