using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoNext.Platform.AccessControl.API.Models.Entities
{
    [Table("users")]
    public class User : BaseEntity
    {
        [Required]
        [Column("email")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Column("phone_number")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        [Column("password_hash")]
        [MaxLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("first_name")]
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [Column("last_name")]
        [MaxLength(100)]
        public string? LastName { get; set; }

        [Column("user_type")]
        [MaxLength(50)]
        public string UserType { get; set; } = "Customer";

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("email_verified")]
        public bool EmailVerified { get; set; }

        [Column("phone_verified")]
        public bool PhoneVerified { get; set; }

        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        [Column("metadata")]
        public string? Metadata { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
    }
}
