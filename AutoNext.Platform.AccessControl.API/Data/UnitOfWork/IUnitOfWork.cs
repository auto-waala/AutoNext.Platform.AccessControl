using AutoNext.Platform.AccessControl.API.Data.Repositories;

namespace AutoNext.Platform.AccessControl.API.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IPermissionRepository Permissions { get; }
        IOrganizationRepository Organizations { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        IUserRoleRepository UserRoles { get; }
        IRolePermissionRepository RolePermissions { get; }
        IUserOrganizationRepository UserOrganizations { get; }
        IOtpVerificationRepository OtpVerifications { get; }
        IUserSessionRepository UserSessions { get; }
        IPasswordResetTokenRepository PasswordResetTokens { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        bool HasActiveTransaction { get; }
    }
}
