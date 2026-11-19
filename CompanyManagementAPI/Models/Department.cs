using System.ComponentModel.DataAnnotations;

namespace CompanyManagementAPI.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Department name is required.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property: One Department has many Employees
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
