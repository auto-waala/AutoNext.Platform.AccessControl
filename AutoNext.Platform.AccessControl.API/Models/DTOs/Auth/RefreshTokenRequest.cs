using System.ComponentModel.DataAnnotations;

namespace AutoNext.Platform.AccessControl.API.Models.DTOs
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
