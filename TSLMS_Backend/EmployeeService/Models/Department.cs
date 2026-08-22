using System.ComponentModel.DataAnnotations;

namespace EmployeeService.Models
{
    public class Department
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        // Navigation
        public ICollection<User> Users { get; set; }
    }
}