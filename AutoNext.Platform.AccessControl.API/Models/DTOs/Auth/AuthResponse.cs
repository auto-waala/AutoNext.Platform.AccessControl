namespace AutoNext.Platform.AccessControl.API.Models.DTOs
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserInfoDto User { get; set; } = new UserInfoDto();
    }


    public class ForgotPasswordResponse
    {
        public bool? IsValid { get; set; } = false;
        public string ResetPasswordToken { get; set; } = string.Empty;
    }

}
