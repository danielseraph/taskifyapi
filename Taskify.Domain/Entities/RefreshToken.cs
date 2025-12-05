using System;

namespace Taskify.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public string? CreatedByIp { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }
        public string? ReasonRevoked { get; set; }

        public string? AppUserId { get; set; }
        public AppUser? AppUser { get; set; }

        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsActive => RevokedAt == null && !IsExpired;
    }
}
