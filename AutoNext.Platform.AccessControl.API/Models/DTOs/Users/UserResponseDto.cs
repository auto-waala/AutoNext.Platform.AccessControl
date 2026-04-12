namespace AutoNext.Platform.AccessControl.API.Models.DTOs
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string UserType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool EmailVerified { get; set; }
        public bool PhoneVerified { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public List<string> Permissions { get; set; } = new List<string>();
        public List<OrganizationDto> Organizations { get; set; } = new List<OrganizationDto>();
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
