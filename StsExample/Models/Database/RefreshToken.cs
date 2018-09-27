using System;
using System.ComponentModel.DataAnnotations;

namespace StsExample.Models.Database
{
    public class RefreshToken
    {
        [Key]
        public string Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Subject { get; set; }

        [MaxLength(50)]
        public string ClientId { get; set; }
        public DateTime IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
        [Required]
        public string ProtectedTicket { get; set; }

        public bool HasExpired()
        {
            return ExpiresUtc <= DateTime.UtcNow;
        }
    }
}