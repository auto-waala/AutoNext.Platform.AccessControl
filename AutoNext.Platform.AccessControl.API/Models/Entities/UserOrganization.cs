using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoNext.Platform.AccessControl.API.Models.Entities
{
    [Table("user_organizations")]
    public class UserOrganization : BaseEntity
    {
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("organization_id")]
        public Guid OrganizationId { get; set; }

        [Column("is_primary")]
        public bool IsPrimary { get; set; }

        [Column("status")]
        [MaxLength(50)]
        public string Status { get; set; } = "Active"; // Active, Suspended, Invited

        [Column("joined_at")]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public virtual Organization? Organization { get; set; }
    }
}
