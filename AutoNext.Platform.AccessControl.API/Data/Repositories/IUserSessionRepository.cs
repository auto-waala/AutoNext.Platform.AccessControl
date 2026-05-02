using AutoNext.Platform.AccessControl.API.Models.Entities;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public interface IUserSessionRepository : IRepository<UserSession>
    {
        Task<UserSession?> GetActiveSessionByTokenAsync(string accessTokenHash);
        Task<IEnumerable<UserSession>> GetActiveUserSessionsAsync(Guid userId);
        Task InvalidateUserSessionsAsync(Guid userId);
        Task InvalidateSessionAsync(Guid sessionId);
        Task CleanupExpiredSessionsAsync();

    }
}
