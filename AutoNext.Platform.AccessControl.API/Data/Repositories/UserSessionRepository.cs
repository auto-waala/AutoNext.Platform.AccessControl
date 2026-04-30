using AutoNext.Platform.AccessControl.API.Data.Context;
using AutoNext.Platform.AccessControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public class UserSessionRepository : Repository<UserSession>, IUserSessionRepository
    {
        public UserSessionRepository(IdentityDbContext context) : base(context)
        {
        }

        public async Task<UserSession?> GetActiveSessionByTokenAsync(string accessTokenHash)
        {
            return await _dbSet
                .Include(us => us.User)
                .FirstOrDefaultAsync(us => us.AccessTokenHash == accessTokenHash
                    && us.IsActive
                    && us.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<IEnumerable<UserSession>> GetActiveUserSessionsAsync(Guid userId)
        {
            return await _dbSet
                .Where(us => us.UserId == userId && us.IsActive && us.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task InvalidateUserSessionsAsync(Guid userId)
        {
            var sessions = await _dbSet
                .Where(us => us.UserId == userId && us.IsActive)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
            }
        }

        public async Task InvalidateSessionAsync(Guid sessionId)
        {
            var session = await GetByIdAsync(sessionId);
            if (session != null)
            {
                session.IsActive = false;
            }
        }

        public async Task CleanupExpiredSessionsAsync()
        {
            var expiredSessions = await _dbSet
                .Where(us => us.ExpiresAt < DateTime.UtcNow || !us.IsActive)
                .ToListAsync();

            if (expiredSessions.Any())
            {
                RemoveRange(expiredSessions);
            }
        }
    }
}
