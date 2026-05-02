using AutoNext.Platform.AccessControl.API.Models.Entities;
using AutoNext.Platform.AccessControl.API.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{

    public class PasswordResetTokenRepository : Repository<PasswordResetToken>, IPasswordResetTokenRepository
    {
        public PasswordResetTokenRepository(IdentityDbContext context)
            : base(context)
        {
        }

        public async Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
        }

        public async Task<PasswordResetToken?> GetValidTokenAsync(string tokenHash)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.TokenHash == tokenHash && !x.IsUsed && x.ExpiresAt > DateTime.UtcNow);
        }

        // 🔹 Get latest active token for a user
        public async Task<PasswordResetToken?> GetActiveTokenAsync(Guid userId)
        {
            return await _dbSet
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsUsed &&
                    x.ExpiresAt > DateTime.UtcNow
                )
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        // 🔹 Mark token as used
        public async Task MarkAsUsedAsync(Guid tokenId)
        {
            var token = await _dbSet
                .FirstOrDefaultAsync(x => x.Id == tokenId);

            if (token != null)
            {
                token.IsUsed = true;
                token.UsedAt = DateTime.UtcNow;

                _dbSet.Update(token);
            }
        }

        // 🔹 Invalidate all tokens for a user
        public async Task InvalidateUserTokensAsync(Guid userId)
        {
            var tokens = await _dbSet
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsUsed
                )
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsUsed = true;
                token.UsedAt = DateTime.UtcNow;
            }

            _dbSet.UpdateRange(tokens);
        }

        // 🔹 Delete expired tokens (for cleanup job)
        public async Task DeleteExpiredTokensAsync()
        {
            var expiredTokens = await _dbSet
                .Where(x => x.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            if (expiredTokens.Any())
            {
                _dbSet.RemoveRange(expiredTokens);
            }
        }
    }
}
