using AutoNext.Platform.AccessControl.API.Data.Context;
using AutoNext.Platform.AccessControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public class PermissionRepository : Repository<Permission>, IPermissionRepository
    {
        public PermissionRepository(IdentityDbContext context) : base(context)
        {
        }

        public async Task<Permission?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.Code == code);
        }

        public async Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(Guid roleId)
        {
            return await _dbSet
                .Where(p => p.RolePermissions.Any(rp => rp.RoleId == roleId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Permission>> GetPermissionsByUserAsync(Guid userId)
        {
            return await _dbSet
                .Where(p => p.RolePermissions.Any(rp => rp.Role!.UserRoles.Any(ur => ur.UserId == userId)))
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<Permission>> GetPermissionsByResourceAsync(string resource)
        {
            return await _dbSet
                .Where(p => p.Resource == resource && p.IsActive)
                .OrderBy(p => p.Action)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetPermissionCodesByUserAsync(Guid userId)
        {
            return await _dbSet
                .Where(p => p.RolePermissions.Any(rp => rp.Role!.UserRoles.Any(ur => ur.UserId == userId)))
                .Select(p => p.Code)
                .Distinct()
                .ToListAsync();
        }
    }
}
