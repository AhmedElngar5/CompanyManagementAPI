namespace CompanyManagementAPI.DTOs.Department
{
    /// <summary>
    /// DTO for returning department data in API responses.
    /// </summary>
    public class DepartmentResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public int EmployeeCount { get; set; }
        public List<EmployeeSummaryDto>? Employees { get; set; }
    }

    /// <summary>
    /// Lightweight employee summary used inside department responses to avoid circular references.
    /// </summary>
    public class EmployeeSummaryDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public DateTime HireDate { get; set; }
    }
}
