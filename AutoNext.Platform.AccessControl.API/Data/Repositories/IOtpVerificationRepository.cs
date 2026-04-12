using AutoNext.Platform.AccessControl.API.Models.Entities;

namespace AutoNext.Platform.AccessControl.API.Data.Repositories
{
    public interface IOtpVerificationRepository : IRepository<OtpVerification>
    {
        Task<OtpVerification?> GetValidOtpAsync(Guid userId, string purpose, string otpCodeHash);
        Task InvalidateOtpAsync(Guid userId, string purpose);
        Task CleanupExpiredOtpsAsync();
    }
}
