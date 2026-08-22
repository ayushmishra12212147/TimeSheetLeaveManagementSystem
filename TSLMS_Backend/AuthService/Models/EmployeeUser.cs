using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Models
{
    [Table("Users")]
    public class EmployeeUser
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(20)]
        public string EmployeeId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Role { get; set; } = string.Empty;

        public Guid? DepartmentId { get; set; }
        public Guid? ManagerId { get; set; }
        public string Password { get; set; } = string.Empty;
        [MaxLength(20)]
        public string? Gender { get; set; }

        public bool IsFirstLogin { get; set; }
        public bool MustResetPassword { get; set; }
        public bool IsProfileComplete { get; set; }
        public DateTime? TempPasswordExpiry { get; set; }
    }
}
