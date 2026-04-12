using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoNext.Platform.AccessControl.API.Models.Entities
{
    [Table("permissions")]
    public class Permission : BaseEntity
    {
        [Required]
        [Column("name")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("code")]
        [MaxLength(100)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [Column("resource")]
        [MaxLength(100)]
        public string Resource { get; set; } = string.Empty;

        [Required]
        [Column("action")]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
