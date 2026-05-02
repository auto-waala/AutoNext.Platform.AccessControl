using AutoNext.Platform.AccessControl.API.Models.DTOs;

namespace AutoNext.Platform.AccessControl.API.Managers.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request);
        Task<bool> LogoutAsync(Guid userId, string refreshToken);
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
        Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
        Task<AuthResponse?> GoogleLoginAsync(GoogleLoginRequest request);
        Task<bool> SendVerificationOtpAsync(string email, string purpose);
        Task<bool> VerifyOtpAsync(VerifyOtpRequest request);
    }
}
