using AutoNext.Platform.AccessControl.API.Data.Context;
using AutoNext.Platform.AccessControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public class OtpVerificationRepository : Repository<OtpVerification>, IOtpVerificationRepository
    {
        public OtpVerificationRepository(IdentityDbContext context) : base(context)
        {
        }

        public async Task<OtpVerification?> GetValidOtpAsync(Guid userId, string purpose, string otpCodeHash)
        {
            return await _dbSet
                .FirstOrDefaultAsync(o => o.UserId == userId
                    && o.Purpose == purpose
                    && o.OtpCodeHash == otpCodeHash
                    && !o.IsUsed
                    && o.ExpiresAt > DateTime.UtcNow);
        }

        public async Task InvalidateOtpAsync(Guid userId, string purpose)
        {
            var otps = await _dbSet
                .Where(o => o.UserId == userId && o.Purpose == purpose && !o.IsUsed)
                .ToListAsync();

            foreach (var otp in otps)
            {
                otp.IsUsed = true;
                otp.UsedAt = DateTime.UtcNow;
            }
        }

        public async Task CleanupExpiredOtpsAsync()
        {
            var expiredOtps = await _dbSet
                .Where(o => o.ExpiresAt < DateTime.UtcNow || o.IsUsed)
                .ToListAsync();

            if (expiredOtps.Any())
            {
                RemoveRange(expiredOtps);
            }
        }
    }
}
