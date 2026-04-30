using AutoNext.Platform.AccessControl.API.Data.Context;
using AutoNext.Platform.AccessControl.API.Data.Repositories;
using AutoNext.Platform.AccessControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AutoNext.Platform.AccessControl.API.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IdentityDbContext _context;
        private IDbContextTransaction? _currentTransaction;

        private IUserRepository? _userRepository;
        private IRoleRepository? _roleRepository;
        private IPermissionRepository? _permissionRepository;
        private IOrganizationRepository? _organizationRepository;
        private IRefreshTokenRepository? _refreshTokenRepository;
        private IUserRoleRepository? _userRoleRepository;
        private IRolePermissionRepository? _rolePermissionRepository;
        private IUserOrganizationRepository? _userOrganizationRepository;
        private IOtpVerificationRepository? _otpVerificationRepository;
        private IUserSessionRepository? _userSessionRepository;

        public UnitOfWork(IdentityDbContext context)
        {
            _context = context;
        }

        public IUserRepository Users => _userRepository ??= new UserRepository(_context);

        public IRoleRepository Roles => _roleRepository ??= new RoleRepository(_context);

        public IPermissionRepository Permissions => _permissionRepository ??= new PermissionRepository(_context);

        public IOrganizationRepository Organizations => _organizationRepository ??= new OrganizationRepository(_context);

        public IRefreshTokenRepository RefreshTokens => _refreshTokenRepository ??= new RefreshTokenRepository(_context);

        public IUserRoleRepository UserRoles => _userRoleRepository ??= new UserRoleRepository(_context);

        public IRolePermissionRepository RolePermissions => _rolePermissionRepository ??= new RolePermissionRepository(_context);

        public IUserOrganizationRepository UserOrganizations => _userOrganizationRepository ??= new UserOrganizationRepository(_context);

        public IOtpVerificationRepository OtpVerifications => _otpVerificationRepository ??= new OtpVerificationRepository(_context);

        public IUserSessionRepository UserSessions => _userSessionRepository ??= new UserSessionRepository(_context);

        public bool HasActiveTransaction => _currentTransaction != null;

        public async Task<int> SaveChangesAsync()
        {
            // Update audit fields
            var entries = _context.ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                if (entry.Entity is BaseEntity entity)
                {
                    entity.UpdatedAt = DateTime.UtcNow;
                }


            }

            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
                return;

            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("No transaction in progress");

            try
            {
                await SaveChangesAsync();
                await _currentTransaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction == null)
                return;

            try
            {
                await _currentTransaction.RollbackAsync();
            }
            finally
            {
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }
        }

        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
        }
    }
}
