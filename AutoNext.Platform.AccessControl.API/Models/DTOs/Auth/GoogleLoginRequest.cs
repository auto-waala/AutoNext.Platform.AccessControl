using System.ComponentModel.DataAnnotations;

namespace AutoNext.Platform.AccessControl.API.Models.DTOs
{
    public class GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}
