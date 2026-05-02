using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoNext.Platform.AccessControl.API.Models.Entities
{
    [Table("password_reset_tokens")]
    public class PasswordResetToken : BaseEntity
    {
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("token_hash")]
        [MaxLength(500)]
        public string TokenHash { get; set; } = string.Empty;

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("is_used")]
        public bool IsUsed { get; set; } = false;

        [Column("used_at")]
        public DateTime? UsedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
