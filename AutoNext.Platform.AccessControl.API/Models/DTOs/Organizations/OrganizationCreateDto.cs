using System.ComponentModel.DataAnnotations;

namespace AutoNext.Platform.AccessControl.API.Models.DTOs
{
    public class OrganizationCreateDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        public string OrganizationType { get; set; } = "Individual";

        public string? Address { get; set; }

        [EmailAddress]
        [MaxLength(255)]
        public string? ContactEmail { get; set; }

        [MaxLength(20)]
        public string? ContactPhone { get; set; }

        public List<Guid>? AdminUserIds { get; set; }

        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class OrganizationUpdateDto
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? Code { get; set; }

        public string? OrganizationType { get; set; }

        public string? Address { get; set; }

        [EmailAddress]
        public string? ContactEmail { get; set; }

        [MaxLength(20)]
        public string? ContactPhone { get; set; }

        public bool? IsActive { get; set; }

        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class OrganizationResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string OrganizationType { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<UserResponseDto> Users { get; set; } = new List<UserResponseDto>();
        public int UserCount { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class OrganizationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string OrganizationType { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
