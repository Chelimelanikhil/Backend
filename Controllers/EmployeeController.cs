namespace AppBackend.Controllers
{
    using AppBackend.Objects;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;
    using MySql.Data.MySqlClient;
    using System.Data;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public EmployeesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _configuration = configuration;
        }


        [HttpGet("GetEmployeeDetails")]
        public async Task<IActionResult> GetEmployeeDetails([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out var validatedToken);

                if (validatedToken == null || validatedToken is not JwtSecurityToken)
                {
                    return Unauthorized("Invalid token.");
                }

                var claims = principal.Claims;
                var employeeIdClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                var tenantIdClaim = claims.FirstOrDefault(c => c.Type == "TenantID");

                if (employeeIdClaim == null || tenantIdClaim == null)
                {
                    return Unauthorized("Invalid token claims.");
                }

                int employeeId = int.Parse(employeeIdClaim.Value);
                int tenantId = int.Parse(tenantIdClaim.Value);

                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                SELECT EmployeeID, TenantID, DepartmentID, FirstName, LastName, Email, PhoneNumber, 
                       DateOfBirth, DateOfJoining, WeekOff1, WeekOff2, JobTitle, Salary, Address, 
                       City, State, ZipCode, Country, Status,BranchID,shiftId
                FROM Employees
                WHERE EmployeeID = @EmployeeID AND TenantID = @TenantID";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeID", employeeId);
                command.Parameters.AddWithValue("@TenantID", tenantId);

                using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return NotFound("Employee not found.");
                }

                var employee = new Employee
                {
                    EmployeeID = reader.GetInt32("EmployeeID"),
                    TenantID = reader.IsDBNull("TenantID") ? (int?)null : reader.GetInt32("TenantID"),
                    DepartmentID = reader.IsDBNull("DepartmentID") ? (int?)null : reader.GetInt32("DepartmentID"),
                    FirstName = reader.GetString("FirstName"),
                    LastName = reader.GetString("LastName"),
                    Email = reader.GetString("Email"),
                    PhoneNumber = reader.IsDBNull("PhoneNumber") ? null : reader.GetString("PhoneNumber"),
                    DateOfBirth = reader.GetDateTime("DateOfBirth"),
                    DateOfJoining = reader.GetDateTime("DateOfJoining"),
                    WeekOff1 = reader.IsDBNull("WeekOff1") ? null : reader.GetString("WeekOff1"),
                    WeekOff2 = reader.IsDBNull("WeekOff2") ? null : reader.GetString("WeekOff2"),
                    JobTitle = reader.IsDBNull("JobTitle") ? null : reader.GetString("JobTitle"),
                    Salary = reader.IsDBNull("Salary") ? (decimal?)null : reader.GetDecimal("Salary"),
                    Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                    City = reader.IsDBNull("City") ? null : reader.GetString("City"),
                    State = reader.IsDBNull("State") ? null : reader.GetString("State"),
                    ZipCode = reader.IsDBNull("ZipCode") ? null : reader.GetString("ZipCode"),
                    Country = reader.IsDBNull("Country") ? null : reader.GetString("Country"),
                    Status = reader.GetString("Status"),
                    BranchId = reader.GetInt32("BranchID"),
                    ShiftId = reader.GetInt32("shiftId")
                };

                return Ok(employee);
            }
            catch (Exception ex)
            {
                return Unauthorized("Token validation failed.");
            }
        }
    }

}
