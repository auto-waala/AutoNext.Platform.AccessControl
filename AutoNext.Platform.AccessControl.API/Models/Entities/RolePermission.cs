using System.ComponentModel.DataAnnotations.Schema;

namespace AutoNext.Platform.AccessControl.API.Models.Entities
{
    [Table("role_permissions")]
    public class RolePermission : BaseEntity
    {
        [Column("role_id")]
        public Guid RoleId { get; set; }

        [Column("permission_id")]
        public Guid PermissionId { get; set; }

        [Column("granted_at")]
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        [Column("granted_by")]
        public Guid? GrantedBy { get; set; }

        // Navigation properties
        [ForeignKey(nameof(RoleId))]
        public virtual Role? Role { get; set; }

        [ForeignKey(nameof(PermissionId))]
        public virtual Permission? Permission { get; set; }
    }
}
