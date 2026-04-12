using AutoNext.Platform.AccessControl.API.Data.Context;
using AutoNext.Platform.AccessControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(IdentityDbContext context) : base(context)
        {
        }
        public async Task AddUserRoleAsync(UserRole userRole)
        {
            await _dbSet.AddAsync(userRole);
        }
        public async Task<IEnumerable<UserRole>> GetUserRolesByUserAsync(Guid userId)
        {
            return await _dbSet
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserRole>> GetUserRolesByRoleAsync(Guid roleId)
        {
            return await _dbSet
                .Include(ur => ur.User)
                .Where(ur => ur.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId, Guid? organizationId = null)
        {
            var query = _dbSet.Where(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (organizationId.HasValue)
                query = query.Where(ur => ur.OrganizationId == organizationId.Value);
            else
                query = query.Where(ur => ur.OrganizationId == null);

            return await query.FirstOrDefaultAsync();
        }

        public async Task RemoveUserRolesAsync(Guid userId)
        {
            var userRoles = await _dbSet.Where(ur => ur.UserId == userId).ToListAsync();
            if (userRoles.Any())
            {
                _dbSet.RemoveRange(userRoles);
            }
        }

        public async Task RemoveUserRoleAsync(Guid userId, Guid roleId, Guid? organizationId = null)
        {
            var userRole = await GetUserRoleAsync(userId, roleId, organizationId);
            if (userRole != null)
            {
                _dbSet.Remove(userRole);
            }
        }
    }
}
