using AutoNext.Platform.AccessControl.API.Managers.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace AutoNext.Platform.AccessControl.API.Managers.Services
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<TokenBlacklistService> _logger;

        public TokenBlacklistService(IMemoryCache cache, ILogger<TokenBlacklistService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public Task BlacklistTokenAsync(string token, TimeSpan expiry)
        {
            var tokenHash = ComputeHash(token);
            _cache.Set(tokenHash, true, expiry);
            _logger.LogDebug("Token blacklisted: {TokenHash}", tokenHash);
            return Task.CompletedTask;
        }

        public Task<bool> IsTokenBlacklistedAsync(string token)
        {
            var tokenHash = ComputeHash(token);
            var isBlacklisted = _cache.TryGetValue(tokenHash, out _);
            return Task.FromResult(isBlacklisted);
        }

        public Task RemoveExpiredTokensAsync()
        {
            // MemoryCache automatically removes expired entries
            return Task.CompletedTask;
        }

        private string ComputeHash(string token)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hash);
        }
    }
}
