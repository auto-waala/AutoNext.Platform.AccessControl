using AutoNext.Platform.AccessControl.API.Data.Context;
using AutoNext.Platform.AccessControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(IdentityDbContext context) : base(context)
        {
        }

        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
        {
            return await _dbSet
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserAsync(Guid userId)
        {
            return await _dbSet
                .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task RevokeAllUserTokensAsync(Guid userId)
        {
            var tokens = await _dbSet
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }
        }

        public async Task RevokeTokenAsync(Guid tokenId)
        {
            var token = await GetByIdAsync(tokenId);
            if (token != null)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var expiredTokens = await _dbSet
                .Where(rt => rt.ExpiresAt < DateTime.UtcNow || rt.IsRevoked)
                .ToListAsync();

            if (expiredTokens.Any())
            {
                RemoveRange(expiredTokens);
            }
        }
    }
}
