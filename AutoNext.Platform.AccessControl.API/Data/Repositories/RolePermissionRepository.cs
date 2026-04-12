using AutoNext.Platform.AccessControl.API.Data.Context;
using AutoNext.Platform.AccessControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public class RolePermissionRepository : Repository<RolePermission>, IRolePermissionRepository
    {
        public RolePermissionRepository(IdentityDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<RolePermission>> GetRolePermissionsByRoleAsync(Guid roleId)
        {
            return await _dbSet
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<IEnumerable<RolePermission>> GetRolePermissionsByPermissionAsync(Guid permissionId)
        {
            return await _dbSet
                .Include(rp => rp.Role)
                .Where(rp => rp.PermissionId == permissionId)
                .ToListAsync();
        }

        public async Task<RolePermission?> GetRolePermissionAsync(Guid roleId, Guid permissionId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
        }

        public async Task RemoveRolePermissionsAsync(Guid roleId)
        {
            var rolePermissions = await _dbSet.Where(rp => rp.RoleId == roleId).ToListAsync();
            if (rolePermissions.Any())
            {
                _dbSet.RemoveRange(rolePermissions);
            }
        }

        public async Task RemoveRolePermissionAsync(Guid roleId, Guid permissionId)
        {
            var rolePermission = await GetRolePermissionAsync(roleId, permissionId);
            if (rolePermission != null)
            {
                _dbSet.Remove(rolePermission);
            }
        }
    }
}
