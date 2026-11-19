using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompanyManagementAPI.Data;
using CompanyManagementAPI.Models;
using CompanyManagementAPI.DTOs.Department;

namespace CompanyManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/departments
        // Returns all departments with their employees using Include()
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentResponseDto>>> GetDepartments()
        {
            var departments = await _context.Departments
                .Include(d => d.Employees)
                .ToListAsync();

            var response = departments.Select(MapToDepartmentResponse).ToList();

            return Ok(response);
        }

        // GET: api/departments/{id}
        // Returns a single department by ID, 404 if not found
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DepartmentResponseDto>> GetDepartment(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return NotFound(new { message = $"Department with ID {id} was not found." });
            }

            return Ok(MapToDepartmentResponse(department));
        }

        // POST: api/departments
        // Creates a new department, returns 201 Created
        [HttpPost]
        public async Task<ActionResult<DepartmentResponseDto>> CreateDepartment(CreateDepartmentDto dto)
        {
            var department = new Department
            {
                Name = dto.Name,
                Description = dto.Description,
                Location = dto.Location,
                CreatedAt = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            var response = MapToDepartmentResponse(department);

            return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, response);
        }

        // PUT: api/departments/{id}
        // Updates an existing department, returns 204 No Content
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateDepartment(int id, UpdateDepartmentDto dto)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                return NotFound(new { message = $"Department with ID {id} was not found." });
            }

            department.Name = dto.Name;
            department.Description = dto.Description;
            department.Location = dto.Location;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/departments/{id}
        // Deletes a department only if it has no employees
        // Returns 400 if employees exist, 404 if not found, 204 on success
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return NotFound(new { message = $"Department with ID {id} was not found." });
            }

            if (department.Employees.Any())
            {
                return BadRequest(new
                {
                    message = $"Cannot delete department '{department.Name}' because it still has {department.Employees.Count} employee(s). Remove all employees first."
                });
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/departments/{departmentId}/employees
        // Returns all employees belonging to a specific department
        [HttpGet("{departmentId:int}/employees")]
        public async Task<ActionResult<IEnumerable<EmployeeSummaryDto>>> GetDepartmentEmployees(int departmentId)
        {
            var departmentExists = await _context.Departments.AnyAsync(d => d.Id == departmentId);

            if (!departmentExists)
            {
                return NotFound(new { message = $"Department with ID {departmentId} was not found." });
            }

            var employees = await _context.Employees
                .Where(e => e.DepartmentId == departmentId)
                .ToListAsync();

            var response = employees.Select(e => new EmployeeSummaryDto
            {
                Id = e.Id,
                FullName = e.FullName,
                Email = e.Email,
                Salary = e.Salary,
                HireDate = e.HireDate
            }).ToList();

            return Ok(response);
        }

        // GET: api/departments/{departmentId}/employees/{employeeId}
        // Nested resource routing: returns an employee only if it belongs to the specified department
        [HttpGet("{departmentId:int}/employees/{employeeId:int}")]
        public async Task<ActionResult<EmployeeSummaryDto>> GetDepartmentEmployee(int departmentId, int employeeId)
        {
            var departmentExists = await _context.Departments.AnyAsync(d => d.Id == departmentId);

            if (!departmentExists)
            {
                return NotFound(new { message = $"Department with ID {departmentId} was not found." });
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == employeeId && e.DepartmentId == departmentId);

            if (employee == null)
            {
                return NotFound(new { message = $"Employee with ID {employeeId} was not found in department {departmentId}." });
            }

            var response = new EmployeeSummaryDto
            {
                Id = employee.Id,
                FullName = employee.FullName,
                Email = employee.Email,
                Salary = employee.Salary,
                HireDate = employee.HireDate
            };

            return Ok(response);
        }

        // --- Helper Methods ---

        private static DepartmentResponseDto MapToDepartmentResponse(Department department)
        {
            return new DepartmentResponseDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                Location = department.Location,
                CreatedAt = department.CreatedAt,
                EmployeeCount = department.Employees?.Count ?? 0,
                Employees = department.Employees?.Select(e => new EmployeeSummaryDto
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    Email = e.Email,
                    Salary = e.Salary,
                    HireDate = e.HireDate
                }).ToList()
            };
        }
    }
}
