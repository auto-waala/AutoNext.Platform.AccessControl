using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoNext.Platform.AccessControl.API.Models.Entities
{
    [Table("organizations")]
    public class Organization : BaseEntity
    {

        [Required]
        [Column("name")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("code")]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Column("organization_type")]
        [MaxLength(50)]
        public string OrganizationType { get; set; } = "Individual"; // HQ, Dealership, Individual

        [Column("address")]
        public string? Address { get; set; }

        [Column("contact_email")]
        [MaxLength(255)]
        public string? ContactEmail { get; set; }

        [Column("contact_phone")]
        [MaxLength(20)]
        public string? ContactPhone { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("metadata")]
        public string? Metadata { get; set; }

        // Navigation properties
        public virtual ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();
    }
}
