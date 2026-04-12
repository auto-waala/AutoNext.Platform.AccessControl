using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoNext.Platform.AccessControl.API.Models.Entities
{
    [Table("otp_verifications")]
    public class OtpVerification : BaseEntity
    {

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("otp_code_hash")]
        [MaxLength(500)]
        public string OtpCodeHash { get; set; } = string.Empty;

        [Column("purpose")]
        [MaxLength(50)]
        public string Purpose { get; set; } = string.Empty; // EmailVerification, PhoneVerification, PasswordReset

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("is_used")]
        public bool IsUsed { get; set; }

        [Column("used_at")]
        public DateTime? UsedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
