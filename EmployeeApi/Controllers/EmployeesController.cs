using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Npgsql;
using EmployeeApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace EmployeeApi.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(IConfiguration configuration, ILogger<EmployeesController> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'DefaultConnection' is missing.");
            _logger = logger;
        }
        /*
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
             var employees = new List<Employee>();
             try
             {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string sql = "SELECT emp_id, emp_name, emp_age, emp_dept, emp_salary, emp_contact_no, emp_hire_date FROM employees";

                    using (var command = new NpgsqlCommand(sql, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            employees.Add(new Employee
                            {
                                EmpId = reader.GetInt32(0),
                                EmpName = reader.IsDBNull(1) ? null : reader.GetString(1),
                                EmpAge = reader.GetInt32(2),
                                EmpDept = reader.IsDBNull(3) ? null : reader.GetString(3),
                                EmpSalary = reader.GetDecimal(4),
                                EmpContactNo = reader.IsDBNull(5) ? null : reader.GetString(5),
                                EmpHireDate = reader.GetDateTime(6)
                            });
                        }
                    }
                }
                return Ok(employees);
             }
             catch (NpgsqlException ex)
             {
                return StatusCode(500, $"Database error: {ex.Message}");
             }
             catch (Exception ex)
             {
                return StatusCode(500, $"Internal server error: {ex.Message}");
             }
        }*/
        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetEmployees([FromQuery] int page = 1, [FromQuery] int perPage = 10,[FromQuery] string? name = null, [FromQuery] string? department = null, [FromQuery] decimal? salary = null,
  [FromQuery] string? dateRange = null, [FromQuery] string? customStart = null, [FromQuery] string? customEnd = null)
        {
            if (page < 1 || perPage < 1)
            {
                return BadRequest("Page and perPage must be positive integers.");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            _logger.LogInformation("Fetching employees for user: {Email}, Page: {Page}, PerPage: {PerPage}", userEmail, page, perPage);

            var employees = new List<Employee>();
            int totalEntries;

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Build filtering where clause dynamically
                    var filters = new List<string>();
                    if (!string.IsNullOrEmpty(name))
                        filters.Add("emp_name ILIKE '%' || @name || '%'");
                    if (!string.IsNullOrEmpty(department))
                        filters.Add("emp_dept ILIKE '%' || @department || '%'");
                    if (salary.HasValue)
                        filters.Add("emp_salary = @salary");
                    // You can add additional filters for date range

                    string whereClause = filters.Count > 0 
                        ? "WHERE " + string.Join(" AND ", filters) 
                        : "";

                    // Get total count
                    string countSql = $"SELECT COUNT(*) FROM employees {whereClause}";
                    using (var countCommand = new NpgsqlCommand(countSql, connection))
                    {
                        if (!string.IsNullOrEmpty(name))
                            countCommand.Parameters.AddWithValue("name", name);
                        if (!string.IsNullOrEmpty(department))
                            countCommand.Parameters.AddWithValue("department", department);
                        if (salary.HasValue)
                            countCommand.Parameters.AddWithValue("salary", salary.Value);
                        totalEntries = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
                    }

                    // Get paginated data
                    int offset = (page - 1) * perPage;
                    string sql = $@"
                        SELECT emp_id, emp_name, emp_age, emp_dept, emp_salary, emp_contact_no, emp_hire_date 
                        FROM employees 
                        {whereClause} 
                        ORDER BY emp_id 
                        LIMIT @perPage OFFSET @offset";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        if (!string.IsNullOrEmpty(name))
                            command.Parameters.AddWithValue("name", name);
                        if (!string.IsNullOrEmpty(department))
                            command.Parameters.AddWithValue("department", department);
                        if (salary.HasValue)
                            command.Parameters.AddWithValue("salary", salary.Value);        

                        command.Parameters.AddWithValue("perPage", perPage);
                        command.Parameters.AddWithValue("offset", offset);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                employees.Add(new Employee
                                {
                                    EmpId = reader.GetInt32(0),
                                    EmpName = reader.IsDBNull(1) ? null : reader.GetString(1),
                                    EmpAge = reader.GetInt32(2),
                                    EmpDept = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    EmpSalary = reader.GetDecimal(4),
                                    EmpContactNo = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    EmpHireDate = reader.GetDateTime(6)
                                });
                            }
                        }
                    }
                }

                var response = new ApiResponse
                {
                    Data = employees,
                    TotalEntries = totalEntries
                };
                _logger.LogInformation("Returning {Count} employees for user: {Email}", employees.Count, userEmail);
                return Ok(response);
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Database error for user: {Email}", userEmail);
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error for user: {Email}", userEmail);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            _logger.LogInformation("Fetching employee ID: {Id} for user: {Email}", id, userEmail);

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string sql = "SELECT emp_id, emp_name, emp_age, emp_dept, emp_salary, emp_contact_no, emp_hire_date FROM employees WHERE emp_id = @id";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("id", id);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var employee = new Employee
                                {
                                    EmpId = reader.GetInt32(0),
                                    EmpName = reader.IsDBNull(1) ? null : reader.GetString(1),
                                    EmpAge = reader.GetInt32(2),
                                    EmpDept = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    EmpSalary = reader.GetDecimal(4),
                                    EmpContactNo = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    EmpHireDate = reader.GetDateTime(6)
                                };
                                _logger.LogInformation("Found employee ID: {Id} for user: {Email}", id, userEmail);
                                return Ok(employee);
                            }
                            _logger.LogWarning("Employee ID: {Id} not found for user: {Email}", id, userEmail);
                            return NotFound($"Employee with ID {id} not found.");
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Database error for user: {Email}, ID: {Id}", userEmail, id);
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error for user: {Email}, ID: {Id}", userEmail, id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
    public class ApiResponse
    {
        public List<Employee> Data { get; set; } = new List<Employee>();
        public int TotalEntries { get; set; }
    }
}