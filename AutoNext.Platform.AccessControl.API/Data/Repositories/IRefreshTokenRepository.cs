using AutoNext.Platform.AccessControl.API.Models.Entities;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
        Task<IEnumerable<RefreshToken>> GetActiveTokensByUserAsync(Guid userId);
        Task RevokeAllUserTokensAsync(Guid userId);
        Task RevokeTokenAsync(Guid tokenId);
        Task CleanupExpiredTokensAsync();
    }
}
