using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        [MaxLength(128)]
        public string TokenHash { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }

        [MaxLength(64)]
        public string? CreatedByIp { get; set; }
    }
}
