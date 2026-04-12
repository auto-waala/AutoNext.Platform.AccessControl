using System.ComponentModel.DataAnnotations;

namespace AutoNext.Platform.AccessControl.API.Models.DTOs
{
    public class RoleUpdateDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? Code { get; set; }

        public string? Description { get; set; }

        public int? DisplayOrder { get; set; }

        public bool? IsActive { get; set; }

        public List<Guid>? PermissionIds { get; set; }
    }
}
