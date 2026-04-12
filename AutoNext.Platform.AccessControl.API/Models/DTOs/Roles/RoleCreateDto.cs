using System.ComponentModel.DataAnnotations;

namespace AutoNext.Platform.AccessControl.API.Models.DTOs
{
    public class RoleCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string RoleType { get; set; } = "Custom";

        public int DisplayOrder { get; set; }

        public List<Guid>? PermissionIds { get; set; }
    }
}
