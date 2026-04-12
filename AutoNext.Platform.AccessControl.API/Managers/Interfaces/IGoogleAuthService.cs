using AutoNext.Platform.AccessControl.API.Models.DTOs;

namespace AutoNext.Platform.AccessControl.API.Managers.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<GoogleUserInfo?> ValidateGoogleTokenAsync(string idToken);
        string GetGoogleLoginUrl(string redirectUri);
        Task<GoogleUserInfo?> ExchangeCodeForTokenAsync(string code, string redirectUri);
    }
}
