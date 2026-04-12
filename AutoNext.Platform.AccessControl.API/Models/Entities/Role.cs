using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoNext.Platform.AccessControl.API.Models.Entities
{
    [Table("roles")]
    public class Role : BaseEntity
    {
        [Required]
        [Column("name")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("code")]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("role_type")]
        [MaxLength(50)]
        public string RoleType { get; set; } = "Custom"; // System, Organization, Custom

        [Column("is_system_role")]
        public bool IsSystemRole { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
