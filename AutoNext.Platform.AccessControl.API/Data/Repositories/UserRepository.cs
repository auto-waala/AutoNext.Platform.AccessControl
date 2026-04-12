using AutoNext.Platform.AccessControl.API.Data.Context;
using AutoNext.Platform.AccessControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(IdentityDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserWithRolesAsync(Guid userId)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetUserWithPermissionsAsync(Guid userId)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r!.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleCode)
        {
            return await _dbSet
                .Where(u => u.UserRoles.Any(ur => ur.Role!.Code == roleCode))
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByOrganizationAsync(Guid organizationId)
        {
            return await _dbSet
                .Where(u => u.UserOrganizations.Any(uo => uo.OrganizationId == organizationId))
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByTypeAsync(string userType)
        {
            return await _dbSet
                .Where(u => u.UserType == userType)
                .ToListAsync();
        }

        public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null)
        {
            var query = _dbSet.Where(u => u.Email == email);
            if (excludeUserId.HasValue)
                query = query.Where(u => u.Id != excludeUserId.Value);

            return !await query.AnyAsync();
        }
    }
}
