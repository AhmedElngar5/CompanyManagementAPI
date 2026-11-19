using System.ComponentModel.DataAnnotations;

namespace CompanyManagementAPI.DTOs.Department
{
    /// <summary>
    /// DTO for updating an existing department.
    /// </summary>
    public class UpdateDepartmentDto
    {
        [Required(ErrorMessage = "Department name is required.")]
        [MaxLength(100, ErrorMessage = "Department name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [MaxLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
        public string? Location { get; set; }
    }
}
