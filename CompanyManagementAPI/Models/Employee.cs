using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyManagementAPI.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "A valid email address is required.")]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

        public DateTime HireDate { get; set; } = DateTime.UtcNow;

        // Foreign Key
        [Required]
        public int DepartmentId { get; set; }

        // Navigation property: Each Employee belongs to one Department
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; } = null!;
    }
}
