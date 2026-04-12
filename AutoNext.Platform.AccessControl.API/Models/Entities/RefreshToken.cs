using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoNext.Platform.AccessControl.API.Models.Entities
{
    [Table("refresh_tokens")]
    public class RefreshToken : BaseEntity
    {
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("token_hash")]
        [MaxLength(500)]
        public string TokenHash { get; set; } = string.Empty;

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("is_revoked")]
        public bool IsRevoked { get; set; }

        [Column("revoked_at")]
        public DateTime? RevokedAt { get; set; }


        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
