using AutoNext.Platform.AccessControl.API.Models.Entities;

namespace AutoNext.Platform.AccessControl.API.Managers.Interfaces
{
    public interface ITwoFactorService
    {
        Task<bool> EnableTwoFactorAsync(Guid userId);
        Task<bool> DisableTwoFactorAsync(Guid userId);
        Task<bool> IsTwoFactorEnabledAsync(Guid userId);
        Task<string> GenerateTwoFactorCodeAsync(User user);
        Task<bool> ValidateTwoFactorCodeAsync(User user, string code);
        Task<string> GenerateRecoveryCodesAsync(Guid userId, int count = 10);
        Task<bool> ValidateRecoveryCodeAsync(Guid userId, string recoveryCode);
    }
}
