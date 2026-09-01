using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    public class LoginAttempt
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        [MaxLength(20)]
        public string EmployeeId { get; set; } = string.Empty;

        public DateTime AttemptedAtUtc { get; set; }
        public bool WasSuccessful { get; set; }

        [MaxLength(64)]
        public string? IpAddress { get; set; }
    }
}
