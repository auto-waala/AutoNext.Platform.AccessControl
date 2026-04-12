using AutoNext.Platform.AccessControl.API.Models.Entities;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public interface IOrganizationRepository : IRepository<Organization>
    {
        Task<Organization?> GetByCodeAsync(string code);
        Task<Organization?> GetOrganizationWithUsersAsync(Guid organizationId);
        Task<IEnumerable<Organization>> GetOrganizationsByUserAsync(Guid userId);
        Task<IEnumerable<Organization>> GetOrganizationsByTypeAsync(string organizationType);
        Task<bool> IsCodeUniqueAsync(string code, Guid? excludeOrgId = null);
    }
}
