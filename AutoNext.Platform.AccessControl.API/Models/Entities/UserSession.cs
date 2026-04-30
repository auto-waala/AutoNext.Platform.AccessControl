using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoNext.Platform.AccessControl.API.Models.Entities
{
    [Table("user_sessions")]
    public class UserSession : BaseEntity
    {
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("access_token_hash")]
        [MaxLength(500)]
        public string? AccessTokenHash { get; set; }

        [Column("ip_address")]
        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [Column("user_agent")]
        public string? UserAgent { get; set; }

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
