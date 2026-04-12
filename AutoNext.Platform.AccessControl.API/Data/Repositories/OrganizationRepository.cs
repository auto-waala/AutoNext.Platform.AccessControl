using AutoNext.Platform.AccessControl.API.Data.Context;
using AutoNext.Platform.AccessControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public class OrganizationRepository : Repository<Organization>, IOrganizationRepository
    {
        public OrganizationRepository(IdentityDbContext context) : base(context)
        {
        }

        public async Task<Organization?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(o => o.Code == code);
        }

        public async Task<Organization?> GetOrganizationWithUsersAsync(Guid organizationId)
        {
            return await _dbSet
                .Include(o => o.UserOrganizations)
                    .ThenInclude(uo => uo.User)
                .FirstOrDefaultAsync(o => o.Id == organizationId);
        }

        public async Task<IEnumerable<Organization>> GetOrganizationsByUserAsync(Guid userId)
        {
            return await _dbSet
                .Where(o => o.UserOrganizations.Any(uo => uo.UserId == userId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Organization>> GetOrganizationsByTypeAsync(string organizationType)
        {
            return await _dbSet
                .Where(o => o.OrganizationType == organizationType && o.IsActive)
                .ToListAsync();
        }

        public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeOrgId = null)
        {
            var query = _dbSet.Where(o => o.Code == code);
            if (excludeOrgId.HasValue)
                query = query.Where(o => o.Id != excludeOrgId.Value);

            return !await query.AnyAsync();
        }
    }
}
