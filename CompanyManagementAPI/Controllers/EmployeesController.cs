using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompanyManagementAPI.Data;
using CompanyManagementAPI.Models;
using CompanyManagementAPI.DTOs.Employee;

namespace CompanyManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/employees
        // Returns all employees with pagination and salary sorting
        // Query parameters: pageNumber, pageSize, sortBy (salary), sortOrder (asc/desc)
        [HttpGet]
        public async Task<ActionResult<object>> GetEmployees(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            IQueryable<Employee> query = _context.Employees.Include(e => e.Department);

            // Apply salary sorting
            if (!string.IsNullOrEmpty(sortBy) && sortBy.Equals("salary", StringComparison.OrdinalIgnoreCase))
            {
                query = sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.Salary)
                    : query.OrderBy(e => e.Salary);
            }
            else
            {
                query = query.OrderBy(e => e.Id);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var employees = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new
            {
                data = employees.Select(MapToEmployeeResponse).ToList(),
                pagination = new
                {
                    currentPage = pageNumber,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = totalPages,
                    hasNext = pageNumber < totalPages,
                    hasPrevious = pageNumber > 1
                }
            };

            return Ok(response);
        }

        // GET: api/employees/{id:int}
        // Returns an employee by ID with department information
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EmployeeResponseDto>> GetEmployee(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound(new { message = $"Employee with ID {id} was not found." });
            }

            return Ok(MapToEmployeeResponse(employee));
        }

        // POST: api/employees
        // Creates a new employee with full validation
        [HttpPost]
        public async Task<ActionResult<EmployeeResponseDto>> CreateEmployee(CreateEmployeeDto dto)
        {
            // Validate that DepartmentId exists
            var departmentExists = await _context.Departments.AnyAsync(d => d.Id == dto.DepartmentId);
            if (!departmentExists)
            {
                return BadRequest(new { message = $"Department with ID {dto.DepartmentId} does not exist." });
            }

            // Validate unique email
            var emailExists = await _context.Employees.AnyAsync(e => e.Email.ToLower() == dto.Email.ToLower());
            if (emailExists)
            {
                return BadRequest(new { message = $"An employee with email '{dto.Email}' already exists." });
            }

            // Validate salary > 0
            if (dto.Salary <= 0)
            {
                return BadRequest(new { message = "Salary must be greater than zero." });
            }

            var employee = new Employee
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Salary = dto.Salary,
                HireDate = dto.HireDate ?? DateTime.UtcNow,
                DepartmentId = dto.DepartmentId
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Reload with department info
            await _context.Entry(employee).Reference(e => e.Department).LoadAsync();

            var response = MapToEmployeeResponse(employee);

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, response);
        }

        // PUT: api/employees/{id:int}
        // Updates an existing employee with validation
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDto dto)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound(new { message = $"Employee with ID {id} was not found." });
            }

            // Validate that DepartmentId exists
            var departmentExists = await _context.Departments.AnyAsync(d => d.Id == dto.DepartmentId);
            if (!departmentExists)
            {
                return BadRequest(new { message = $"Department with ID {dto.DepartmentId} does not exist." });
            }

            // Validate unique email (excluding current employee)
            var emailExists = await _context.Employees
                .AnyAsync(e => e.Email.ToLower() == dto.Email.ToLower() && e.Id != id);
            if (emailExists)
            {
                return BadRequest(new { message = $"An employee with email '{dto.Email}' already exists." });
            }

            // Validate salary > 0
            if (dto.Salary <= 0)
            {
                return BadRequest(new { message = "Salary must be greater than zero." });
            }

            employee.FullName = dto.FullName;
            employee.Email = dto.Email;
            employee.Salary = dto.Salary;
            employee.DepartmentId = dto.DepartmentId;

            if (dto.HireDate.HasValue)
            {
                employee.HireDate = dto.HireDate.Value;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/employees/{id:int}
        // Deletes an employee, returns 204 on success, 404 if not found
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound(new { message = $"Employee with ID {id} was not found." });
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/employees/department/{departmentId:int}
        // Returns employees filtered by DepartmentId
        [HttpGet("department/{departmentId:int}")]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetEmployeesByDepartment(int departmentId)
        {
            var departmentExists = await _context.Departments.AnyAsync(d => d.Id == departmentId);
            if (!departmentExists)
            {
                return NotFound(new { message = $"Department with ID {departmentId} was not found." });
            }

            var employees = await _context.Employees
                .Include(e => e.Department)
                .Where(e => e.DepartmentId == departmentId)
                .ToListAsync();

            var response = employees.Select(MapToEmployeeResponse).ToList();

            return Ok(response);
        }

        // GET: api/employees/salary/{salary:decimal}
        // Find employees by exact salary using the decimal route constraint
        [HttpGet("salary/{salary:decimal}")]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetEmployeesBySalary(decimal salary)
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Where(e => e.Salary == salary)
                .ToListAsync();

            var response = employees.Select(MapToEmployeeResponse).ToList();

            return Ok(response);
        }

        // GET: api/employees/search
        // GET: api/employees/search/{name?}
        // Search employees by name with optional route parameter
        // Returns all employees when no name is supplied
        [HttpGet("search/{name?}")]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> SearchEmployees(string? name = null)
        {
            IQueryable<Employee> query = _context.Employees.Include(e => e.Department);

            if (!string.IsNullOrWhiteSpace(name))
            {
                // Case-insensitive name search
                query = query.Where(e => e.FullName.ToLower().Contains(name.ToLower()));
            }

            var employees = await query.ToListAsync();
            var response = employees.Select(MapToEmployeeResponse).ToList();

            return Ok(response);
        }

        // GET: api/employees/search/by-name/{name:alpha}
        // Additional search endpoint with alpha route constraint
        // Only accepts alphabetic characters in the name parameter
        [HttpGet("search/by-name/{name:alpha}")]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> SearchEmployeesByNameAlpha(string name)
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Where(e => e.FullName.ToLower().Contains(name.ToLower()))
                .ToListAsync();

            var response = employees.Select(MapToEmployeeResponse).ToList();

            return Ok(response);
        }

        // GET: api/employees/filter?departmentId=2&minSalary=5000&maxSalary=15000
        // Filter employees using model binding with [FromQuery]
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> FilterEmployees(
            [FromQuery] int? departmentId,
            [FromQuery] decimal? minSalary,
            [FromQuery] decimal? maxSalary)
        {
            // Validate salary range
            if (minSalary.HasValue && maxSalary.HasValue && minSalary > maxSalary)
            {
                return BadRequest(new { message = "minSalary cannot be greater than maxSalary." });
            }

            IQueryable<Employee> query = _context.Employees.Include(e => e.Department);

            // Apply only the filters provided by the client
            if (departmentId.HasValue)
            {
                query = query.Where(e => e.DepartmentId == departmentId.Value);
            }

            if (minSalary.HasValue)
            {
                query = query.Where(e => e.Salary >= minSalary.Value);
            }

            if (maxSalary.HasValue)
            {
                query = query.Where(e => e.Salary <= maxSalary.Value);
            }

            var employees = await query.ToListAsync();
            var response = employees.Select(MapToEmployeeResponse).ToList();

            return Ok(response);
        }

        // --- Helper Methods ---

        private static EmployeeResponseDto MapToEmployeeResponse(Employee employee)
        {
            return new EmployeeResponseDto
            {
                Id = employee.Id,
                FullName = employee.FullName,
                Email = employee.Email,
                Salary = employee.Salary,
                HireDate = employee.HireDate,
                DepartmentId = employee.DepartmentId,
                DepartmentName = employee.Department?.Name ?? string.Empty
            };
        }
    }
}
