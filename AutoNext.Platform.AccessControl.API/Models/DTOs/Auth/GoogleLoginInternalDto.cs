namespace AutoNext.Platform.AccessControl.API.Models.DTOs
{
    public class GoogleLoginInternalDto
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool EmailVerified { get; set; }
    }
}
