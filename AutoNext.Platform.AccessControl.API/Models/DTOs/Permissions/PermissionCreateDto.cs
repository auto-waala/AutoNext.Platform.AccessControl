using System.ComponentModel.DataAnnotations;

namespace AutoNext.Platform.AccessControl.API.Models.DTOs
{
    public class PermissionCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Resource { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        public string? Description { get; set; }
    }

    public class PermissionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
