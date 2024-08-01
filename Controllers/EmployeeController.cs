//namespace AppBackend.Controllers
//{
//    using AppBackend.Manager;
//    using AppBackend.Objects;
//    using Microsoft.AspNetCore.Mvc;
//    using Microsoft.IdentityModel.Tokens;
//    using MySql.Data.MySqlClient;
//    using System.Data;
//    using System.IdentityModel.Tokens.Jwt;
//    using System.Security.Claims;
//    using System.Text;
//    using System.Threading.Tasks;

//    [ApiController]
//    [Route("api/[controller]")]
//    public class EmployeesController : ControllerBase
//    {
//        private readonly string _connectionString;
//        private readonly IConfiguration _configuration;

//        public EmployeesController(IConfiguration configuration)
//        {
//            _connectionString = configuration.GetConnectionString("DefaultConnection");
//            _configuration = configuration;
//        }


//        [HttpGet("GetEmployeeDetails")]
//        public async Task<IActionResult> GetEmployeeDetails([FromQuery] string token)
//        {
//            if (string.IsNullOrEmpty(token))
//            {
//                return BadRequest("Token is required.");
//            }
//            //E9V8ZUKJ5ZZSXYJQ58W7G54B
//            try
//            {
//                var tokenData = ValidateToken(token);
//                if (tokenData == null)
//                {
//                    return Unauthorized("Invalid token.");
//                }

//                var employeeMgr = new EmployeeMgr(_configuration);
//                List<Employee> employee = await employeeMgr.GetEmployeeDetailsAsync(tokenData.EmployeeId, tokenData.TenantId);

//                if (employee == null)
//                {
//                    return NotFound("Employee not found.");
//                }

//                return Ok(employee);
//            }
//            catch (Exception)
//            {
//                return Unauthorized("Token validation failed.");
//            }
//        }

//        private TokenData ValidateToken(string token)
//        {
//            var tokenHandler = new JwtSecurityTokenHandler();
//            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

//            try
//            {
//                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
//                {
//                    ValidateIssuer = true,
//                    ValidateAudience = true,
//                    ValidateLifetime = true,
//                    ValidateIssuerSigningKey = true,
//                    ValidIssuer = _configuration["Jwt:Issuer"],
//                    ValidAudience = _configuration["Jwt:Audience"],
//                    IssuerSigningKey = new SymmetricSecurityKey(key)
//                }, out SecurityToken validatedToken);

//                if (validatedToken != null && validatedToken is JwtSecurityToken jwtToken)
//                {
//                    var employeeIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
//                    var tenantIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "TenantID");

//                    if (employeeIdClaim != null && tenantIdClaim != null)
//                    {
//                        return new TokenData
//                        {
//                            EmployeeId = int.Parse(employeeIdClaim.Value),
//                            TenantId = int.Parse(tenantIdClaim.Value)
//                        };
//                    }
//                }

//                return null;
//            }
//            catch
//            {
//                return null;
//            }
//        }

//    }

//}


using AppBackend.Manager;
using AppBackend.Objects;
using AppBackend.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppBackend.Controllers
{
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

            try
            {
                TokenService teS = new TokenService(_configuration);
                var tokenData = teS.ValidateToken(token);
                if (tokenData == null)
                {
                    return Unauthorized("Invalid token.");
                }

                var employeeMgr = new EmployeeMgr(_configuration);
                List<Employee> employee = await employeeMgr.GetEmployeeDetailsAsync(tokenData.EmployeeId, tokenData.TenantId);

                if (employee == null)
                {
                    return NotFound("Employee not found.");
                }

                return Ok(employee);
            }
            catch (Exception)
            {
                return Unauthorized("Token validation failed.");
            }
        }

        [HttpGet("GetIndividualEmployeeDetails")]
        public async Task<IActionResult> GetIndividualEmployeeDetails([FromQuery] int Id)
        {
            try
            {
                var employeeMgr = new EmployeeMgr(_configuration);
                var employee = await employeeMgr.GetIndividualEmployeeDetails(Id); // Fetch a single employee

                if (employee == null)
                {
                    return NotFound("Employee not found.");
                }

                return Ok(employee);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error."); // Use StatusCode for general exceptions
            }
        }

    }
}
