using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeService.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string EmployeeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; }

        // Foreign Key
        public Guid? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        [Required]
        public string Password { get; set; }

        public bool IsFirstLogin { get; set; } = true;
        public bool MustResetPassword { get; set; } = true;
        public bool IsProfileComplete { get; set; }
        public DateTime? TempPasswordExpiry { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(250)]
        public string? Address { get; set; }

        [MaxLength(500)]
        public string? ProfilePhotoUrl { get; set; }

        [MaxLength(100)]
        public string? EmergencyContactName { get; set; }

        [MaxLength(20)]
        public string? EmergencyContactPhone { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        public Guid? ManagerId { get; set; }

        [ForeignKey("ManagerId")]
        public User Manager { get; set; }

        // Navigation (optional but useful)
        public ICollection<User> Subordinates { get; set; }
    }
}
