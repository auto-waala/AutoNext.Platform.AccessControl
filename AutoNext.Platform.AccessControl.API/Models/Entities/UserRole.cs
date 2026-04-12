using System.ComponentModel.DataAnnotations.Schema;

namespace AutoNext.Platform.AccessControl.API.Models.Entities
{
    [Table("user_roles")]
    public class UserRole : BaseEntity
    {
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("role_id")]
        public Guid RoleId { get; set; }

        [Column("organization_id")]
        public Guid? OrganizationId { get; set; }

        [Column("assigned_at")]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [Column("assigned_by")]
        public Guid? AssignedBy { get; set; }

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(RoleId))]
        public virtual Role? Role { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public virtual Organization? Organization { get; set; }
    }
}
