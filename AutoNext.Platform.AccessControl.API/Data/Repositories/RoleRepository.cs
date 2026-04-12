using AutoNext.Platform.AccessControl.API.Data.Context;
using AutoNext.Platform.AccessControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        public RoleRepository(IdentityDbContext context) : base(context)
        {
        }

        public async Task<Role?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.Code == code);
        }

        public async Task<Role?> GetRoleWithPermissionsAsync(Guid roleId)
        {
            return await _dbSet
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == roleId);
        }

        public async Task<IEnumerable<Role>> GetRolesByUserAsync(Guid userId)
        {
            return await _dbSet
                .Where(r => r.UserRoles.Any(ur => ur.UserId == userId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Role>> GetSystemRolesAsync()
        {
            return await _dbSet
                .Where(r => r.IsSystemRole)
                .OrderBy(r => r.DisplayOrder)
                .ToListAsync();
        }

        public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeRoleId = null)
        {
            var query = _dbSet.Where(r => r.Code == code);
            if (excludeRoleId.HasValue)
                query = query.Where(r => r.Id != excludeRoleId.Value);

            return !await query.AnyAsync();
        }
    }
}
