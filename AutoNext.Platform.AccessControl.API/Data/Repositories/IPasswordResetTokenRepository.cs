using AutoNext.Platform.AccessControl.API.Models.Entities;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken>
    {
        // Get token by hash
        Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash);

        // Get valid token (recommended - includes validation)
        Task<PasswordResetToken?> GetValidTokenAsync(string tokenHash);

        // Get latest active token for a user (optional)
        Task<PasswordResetToken?> GetActiveTokenAsync(Guid userId);

        // Mark token as used
        Task MarkAsUsedAsync(Guid tokenId);

        // Invalidate all tokens for a user
        Task InvalidateUserTokensAsync(Guid userId);

        // Cleanup expired tokens
        Task DeleteExpiredTokensAsync();
    }
}
