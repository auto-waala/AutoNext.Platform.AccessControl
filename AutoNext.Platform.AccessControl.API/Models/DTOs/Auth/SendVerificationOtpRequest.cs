using System.ComponentModel.DataAnnotations;

namespace AutoNext.Platform.AccessControl.API.Models.DTOs
{
    public class SendVerificationOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Purpose { get; set; } = "EmailVerification"; // EmailVerification, PhoneVerification, PasswordReset
    }
}
