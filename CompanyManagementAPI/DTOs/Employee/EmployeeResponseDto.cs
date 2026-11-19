namespace CompanyManagementAPI.DTOs.Employee
{
    /// <summary>
    /// DTO for returning employee data in API responses.
    /// </summary>
    public class EmployeeResponseDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public DateTime HireDate { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
    }
}
