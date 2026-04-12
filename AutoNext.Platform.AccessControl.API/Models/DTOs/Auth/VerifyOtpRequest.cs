using System.ComponentModel.DataAnnotations;

namespace AutoNext.Platform.AccessControl.API.Models.DTOs
{
    public class VerifyOtpRequest
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string OtpCode { get; set; } = string.Empty;

        public string Purpose { get; set; } = "EmailVerification";
    }
}
