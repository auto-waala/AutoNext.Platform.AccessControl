using AutoMapper;
using AutoNext.Platform.AccessControl.API.Data.UnitOfWork;
using AutoNext.Platform.AccessControl.API.Managers.Interfaces;
using AutoNext.Platform.AccessControl.API.Models.DTOs;
using AutoNext.Platform.AccessControl.API.Models.Entities;

namespace AutoNext.Platform.AccessControl.API.Managers.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PermissionService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PermissionDto?> GetPermissionByIdAsync(Guid permissionId)
        {
            var permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId);
            if (permission == null)
                return null;

            return _mapper.Map<PermissionDto>(permission);
        }

        public async Task<PermissionDto?> GetPermissionByCodeAsync(string code)
        {
            var permission = await _unitOfWork.Permissions.GetByCodeAsync(code);
            if (permission == null)
                return null;

            return _mapper.Map<PermissionDto>(permission);
        }

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
        {
            var permissions = await _unitOfWork.Permissions.GetAllAsync();
            return _mapper.Map<IEnumerable<PermissionDto>>(permissions)
                .OrderBy(p => p.Resource)
                .ThenBy(p => p.Action);
        }

        public async Task<IEnumerable<PermissionDto>> GetPermissionsByResourceAsync(string resource)
        {
            var permissions = await _unitOfWork.Permissions.GetPermissionsByResourceAsync(resource);
            return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
        }

        public async Task<PermissionDto> CreatePermissionAsync(PermissionCreateDto createDto)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Check if code exists
                var existingPermission = await _unitOfWork.Permissions.GetByCodeAsync(createDto.Code);
                if (existingPermission != null)
                    throw new InvalidOperationException($"Permission with code {createDto.Code} already exists");

                var permission = _mapper.Map<Permission>(createDto);
                permission.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.Permissions.AddAsync(permission);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Permission created successfully: {Code}", permission.Code);

                return _mapper.Map<PermissionDto>(permission);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> DeletePermissionAsync(Guid permissionId)
        {
            var permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId);
            if (permission == null)
                return false;

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Check if permission is assigned to any role
                var rolePermissions = await _unitOfWork.RolePermissions
                    .FindAsync(rp => rp.PermissionId == permissionId);

                if (rolePermissions.Any())
                    throw new InvalidOperationException("Cannot delete permission that is assigned to roles");

                _unitOfWork.Permissions.Remove(permission);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Permission deleted successfully: {Code}", permission.Code);
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
        {
            return await _unitOfWork.Permissions.GetPermissionCodesByUserAsync(userId);
        }

        public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionCode)
        {
            var permissions = await GetUserPermissionsAsync(userId);
            return permissions.Contains(permissionCode);
        }
    }
}
