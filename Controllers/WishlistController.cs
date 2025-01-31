﻿//using AppBackend.Objects;
//using Microsoft.AspNetCore.Mvc;
//using MySql.Data.MySqlClient;
//using System.Collections.Generic;
//using System.Data;
//using System.Reflection.PortableExecutable;
//using System.Threading.Tasks;
//using System.Xml.Linq;

//namespace AppBackend.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class WishlistController : ControllerBase
//    {
//        private readonly string _connectionString;

//        public WishlistController(IConfiguration configuration)
//        {
//            _connectionString = configuration.GetConnectionString("DefaultConnection");
//        }
//        [HttpGet]
//        [Route("EmployeesOfTheMonth")]
//        public async Task<IActionResult> GetEmployeesOfTheMonth([FromQuery] int tenantId)
//        {
//            var currentDate = DateTime.UtcNow;
//            var currentMonth = currentDate.Month;
//            var currentYear = currentDate.Year;

//            var query = @"
//        SELECT * 
//        FROM Employees 
//        WHERE 
//            TenantID = @TenantID
//            AND (
//                (MONTH(DateOfBirth) = @CurrentMonth AND YEAR(DateOfBirth) = @CurrentYear)
//                OR 
//                (MONTH(DateOfJoining) = @CurrentMonth AND YEAR(DateOfJoining) = @CurrentYear)
//            );
//    ";

//            using var connection = new MySqlConnection(_connectionString);
//            await connection.OpenAsync();

//            using var command = new MySqlCommand(query, connection);
//            command.Parameters.AddWithValue("@TenantID", tenantId);
//            command.Parameters.AddWithValue("@CurrentMonth", currentMonth);
//            command.Parameters.AddWithValue("@CurrentYear", currentYear);

//            using var reader = await command.ExecuteReaderAsync();

//            var results = new List<Wishlist>();
//            while (await reader.ReadAsync())
//            {
//                var record = new Wishlist
//                {

//                    Id = reader.GetInt32("EmployeeID"),
//                    Name = reader.GetString("FirstName"),
//                    DateOfBirth = reader.GetDateTime("DateOfBirth"),
//                    DateOfJoining = reader.GetDateTime("DateOfJoining")
//                };

//                results.Add(record);
//            }

//            if (results.Count == 0)
//            {
//                return NotFound("No employees found for this month.");
//            }

//            return Ok(results);
//        }


//    }


//}
using AppBackend.Data;
using AppBackend.Manager;
using AppBackend.Objects;
using AppBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WishlistController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public WishlistController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _configuration = configuration;
        }
        [HttpGet]
        [Route("EmployeesOfTheMonth")]
        public async Task<IActionResult> GetEmployeesOfTheMonth([FromQuery] string token)
        {
            var currentDate = DateTime.UtcNow;
            var currentMonth = currentDate.Month;

            try
            {
                TokenService teS = new TokenService(_configuration);
                var tokenData = teS.ValidateToken(token);
                if (tokenData == null)
                {
                    return Unauthorized("Invalid token.");
                }

                var wishlistMgr = new WishlistMgr(_configuration);
                EmployeesOfTheMonthResponse employeesResponse = await wishlistMgr.GetEmployeesOfTheMonthAsync(tokenData.TenantId, currentMonth);

                if ((employeesResponse.Birthdays == null || employeesResponse.Birthdays.Count == 0) &&
                    (employeesResponse.Anniversaries == null || employeesResponse.Anniversaries.Count == 0))
                {
                    return Ok(new { Message = "No employees to wish" });
                }

                return Ok(employeesResponse);
            }
            catch (Exception ex)
            {
                // Log exception details here if needed
                return StatusCode(500, "Internal server error");
            }
        }


    }
}

