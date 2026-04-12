using AutoMapper;
using AutoNext.Platform.AccessControl.API.Data.UnitOfWork;
using AutoNext.Platform.AccessControl.API.Managers.Interfaces;
using AutoNext.Platform.AccessControl.API.Models.DTOs;
using AutoNext.Platform.AccessControl.API.Models.Entities;

namespace AutoNext.Platform.AccessControl.API.Managers.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RoleService> _logger;

        public RoleService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<RoleService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<RoleResponseDto?> GetRoleByIdAsync(Guid roleId)
        {
            var role = await _unitOfWork.Roles.GetRoleWithPermissionsAsync(roleId);
            if (role == null)
                return null;

            var dto = _mapper.Map<RoleResponseDto>(role);

            // Load permissions
            var permissions = await _unitOfWork.RolePermissions.GetRolePermissionsByRoleAsync(roleId);
            dto.Permissions = _mapper.Map<List<PermissionDto>>(permissions.Select(rp => rp.Permission));

            // Get user count
            var userRoles = await _unitOfWork.UserRoles.GetUserRolesByRoleAsync(roleId);
            dto.UserCount = userRoles.Select(ur => ur.UserId).Distinct().Count();

            return dto;
        }

        public async Task<RoleResponseDto?> GetRoleByCodeAsync(string code)
        {
            var role = await _unitOfWork.Roles.GetByCodeAsync(code);
            if (role == null)
                return null;

            return await GetRoleByIdAsync(role.Id);
        }

        public async Task<IEnumerable<RoleResponseDto>> GetAllRolesAsync()
        {
            var roles = await _unitOfWork.Roles.GetAllAsync();
            var result = new List<RoleResponseDto>();

            foreach (var role in roles)
            {
                var dto = await GetRoleByIdAsync(role.Id);
                if (dto != null)
                    result.Add(dto);
            }

            return result.OrderBy(r => r.DisplayOrder);
        }

        public async Task<IEnumerable<RoleResponseDto>> GetActiveRolesAsync()
        {
            var roles = await _unitOfWork.Roles.FindAsync(r => r.IsActive);
            var result = new List<RoleResponseDto>();

            foreach (var role in roles)
            {
                var dto = await GetRoleByIdAsync(role.Id);
                if (dto != null)
                    result.Add(dto);
            }

            return result.OrderBy(r => r.DisplayOrder);
        }

        public async Task<RoleResponseDto> CreateRoleAsync(RoleCreateDto createDto)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Check if code exists
                var existingRole = await _unitOfWork.Roles.GetByCodeAsync(createDto.Code);
                if (existingRole != null)
                    throw new InvalidOperationException($"Role with code {createDto.Code} already exists");

                var role = _mapper.Map<Role>(createDto);
                role.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.Roles.AddAsync(role);
                await _unitOfWork.SaveChangesAsync();

                // Assign permissions
                if (createDto.PermissionIds != null && createDto.PermissionIds.Any())
                {
                    await AssignPermissionsAsync(role.Id, createDto.PermissionIds);
                }

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Role created successfully: {Code}", role.Code);

                return await GetRoleByIdAsync(role.Id) ?? throw new Exception("Failed to retrieve created role");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<RoleResponseDto?> UpdateRoleAsync(Guid roleId, RoleUpdateDto updateDto)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
                if (role == null)
                    return null;

                // Check code uniqueness
                if (!string.IsNullOrEmpty(updateDto.Code) && updateDto.Code != role.Code)
                {
                    var existingRole = await _unitOfWork.Roles.GetByCodeAsync(updateDto.Code);
                    if (existingRole != null)
                        throw new InvalidOperationException($"Role with code {updateDto.Code} already exists");
                }

                _mapper.Map(updateDto, role);
                role.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Roles.Update(role);
                await _unitOfWork.SaveChangesAsync();

                // Update permissions if provided
                if (updateDto.PermissionIds != null)
                {
                    await AssignPermissionsAsync(roleId, updateDto.PermissionIds);
                }

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Role updated successfully: {Code}", role.Code);

                return await GetRoleByIdAsync(roleId);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> DeleteRoleAsync(Guid roleId)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
            if (role == null)
                return false;

            if (role.IsSystemRole)
                throw new InvalidOperationException("Cannot delete system roles");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Remove relationships
                await _unitOfWork.RolePermissions.RemoveRolePermissionsAsync(roleId);
                await _unitOfWork.UserRoles.RemoveUserRolesAsync(roleId);

                _unitOfWork.Roles.Remove(role);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Role deleted successfully: {Code}", role.Code);
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> ToggleRoleStatusAsync(Guid roleId, bool isActive)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
            if (role == null)
                return false;

            role.IsActive = isActive;
            role.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Roles.Update(role);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Role {RoleId} status changed to: {Status}", roleId, isActive);
            return true;
        }

        public async Task<bool> AssignPermissionsAsync(Guid roleId, List<Guid> permissionIds)
        {
            // Remove existing permissions
            await _unitOfWork.RolePermissions.RemoveRolePermissionsAsync(roleId);

            // Add new permissions
            foreach (var permissionId in permissionIds)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    GrantedAt = DateTime.UtcNow
                };
                await _unitOfWork.RolePermissions.AddAsync(rolePermission);
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }

}
