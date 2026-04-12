using AutoNext.Platform.AccessControl.API.Data.Context;
using AutoNext.Platform.AccessControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public class UserOrganizationRepository : Repository<UserOrganization>, IUserOrganizationRepository
    {
        public UserOrganizationRepository(IdentityDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserOrganization>> GetUserOrganizationsByUserAsync(Guid userId)
        {
            return await _dbSet
                .Include(uo => uo.Organization)
                .Where(uo => uo.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserOrganization>> GetUserOrganizationsByOrganizationAsync(Guid organizationId)
        {
            return await _dbSet
                .Include(uo => uo.User)
                .Where(uo => uo.OrganizationId == organizationId)
                .ToListAsync();
        }

        public async Task<UserOrganization?> GetUserOrganizationAsync(Guid userId, Guid organizationId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(uo => uo.UserId == userId && uo.OrganizationId == organizationId);
        }

        public async Task<UserOrganization?> GetPrimaryOrganizationAsync(Guid userId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(uo => uo.UserId == userId && uo.IsPrimary);
        }

        public async Task RemoveUserFromOrganizationAsync(Guid userId, Guid organizationId)
        {
            var userOrg = await GetUserOrganizationAsync(userId, organizationId);
            if (userOrg != null)
            {
                _dbSet.Remove(userOrg);
            }
        }

        public async Task SetPrimaryOrganizationAsync(Guid userId, Guid organizationId)
        {
            // Remove primary from all user's organizations
            var userOrgs = await _dbSet.Where(uo => uo.UserId == userId).ToListAsync();
            foreach (var uo in userOrgs)
            {
                uo.IsPrimary = false;
            }

            // Set new primary
            var primaryOrg = await GetUserOrganizationAsync(userId, organizationId);
            if (primaryOrg != null)
            {
                primaryOrg.IsPrimary = true;
            }
        }
    }
}
