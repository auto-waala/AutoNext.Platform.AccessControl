namespace AutoNext.Platform.AccessControl.API.Models.DTOs
{
    public class TwoFactorSetupDto
    {
        public bool IsEnabled { get; set; }
        public string? SecretKey { get; set; }
        public string? QrCodeUrl { get; set; }
        public List<string> RecoveryCodes { get; set; } = new List<string>();
    }

    public class TwoFactorVerifyDto
    {
        public string Code { get; set; } = string.Empty;
    }

    public class TwoFactorResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? RecoveryCode { get; set; }
    }
}
