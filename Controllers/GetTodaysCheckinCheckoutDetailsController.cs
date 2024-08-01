using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

[ApiController]
[Route("[controller]")]
public class GetTodaysCheckinCheckoutDetailsController : ControllerBase
{
    private readonly string _connectionString;

    public GetTodaysCheckinCheckoutDetailsController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    [HttpGet]
    [Route("GetTodaysDetails")]
    public async Task<IActionResult> GetDetails(int tenantId, int employeeId)
    {
        var todayDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var query = @"
            SELECT * 
            FROM CheckInCheckOut 
            WHERE TenantID = @TenantID 
              AND EmployeeID = @EmployeeID 
              AND SUBSTRING(CheckInTime, 1, 10) = @TodayDate
        ";

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@TenantID", tenantId);
        command.Parameters.AddWithValue("@EmployeeID", employeeId);
        command.Parameters.AddWithValue("@TodayDate", todayDate);

        using var reader = await command.ExecuteReaderAsync();

        var results = new List<CheckinCheckout>();
        while (await reader.ReadAsync())
        {
            var record = new CheckinCheckout
            {
                TenantID = reader.GetInt32("TenantID"),
                EmployeeID = reader.GetInt32("EmployeeID"),
                CheckInTime = reader.IsDBNull("CheckInTime") ? (DateTime?)null : reader.GetDateTime("CheckInTime"),
                CheckOutTime = reader.IsDBNull("CheckOutTime") ? (DateTime?)null : reader.GetDateTime("CheckOutTime"),
                CheckInPhoto = reader.IsDBNull("CheckInPhoto") ? null : Convert.ToBase64String((byte[])reader["CheckInPhoto"]),
                CheckOutPhoto = reader.IsDBNull("CheckOutPhoto") ? null : Convert.ToBase64String((byte[])reader["CheckOutPhoto"]),
                CheckInLatitude = reader.IsDBNull("CheckInLatitude") ? (double?)null : reader.GetDouble("CheckInLatitude"),
                CheckInLongitude = reader.IsDBNull("CheckInLongitude") ? (double?)null : reader.GetDouble("CheckInLongitude"),
                CheckOutLatitude = reader.IsDBNull("CheckOutLatitude") ? (double?)null : reader.GetDouble("CheckOutLatitude"),
                CheckOutLongitude = reader.IsDBNull("CheckOutLongitude") ? (double?)null : reader.GetDouble("CheckOutLongitude"),
                CheckInDevice = reader.IsDBNull("CheckInDevice") ? null : reader.GetString("CheckInDevice"),
                CheckOutDevice = reader.IsDBNull("CheckOutDevice") ? null : reader.GetString("CheckOutDevice")
            };

            results.Add(record);
        }

        if (results.Count == 0)
        {
            return NotFound("No check-in/check-out records found for today.");
        }

        return Ok(results);
    }

    [HttpGet]
    [Route("CurrentMonth")]
    public async Task<IActionResult> GetCurrentMonthCheckIns(int tenantId, int employeeId)
    {
        var currentDate = DateTime.UtcNow;
        var currentMonth = currentDate.Month; 
        var currentYear = currentDate.Year;

        var query = @"
            SELECT * 
            FROM CheckInCheckOut 
            WHERE TenantID = @TenantID 
              AND EmployeeID = @EmployeeID 
              AND MONTH(CheckInTime) = @CurrentMonth
              AND YEAR(CheckInTime) = @CurrentYear
        ";

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@TenantID", tenantId);
        command.Parameters.AddWithValue("@EmployeeID", employeeId);
        command.Parameters.AddWithValue("@CurrentMonth", currentMonth);
        command.Parameters.AddWithValue("@CurrentYear", currentYear);

        using var reader = await command.ExecuteReaderAsync();

        var results = new List<CheckinCheckout>();
        while (await reader.ReadAsync())
        {
            var record = new CheckinCheckout
            {
                TenantID = reader.GetInt32("TenantID"),
                EmployeeID = reader.GetInt32("EmployeeID"),
                CheckInTime = reader.IsDBNull("CheckInTime") ? (DateTime?)null : reader.GetDateTime("CheckInTime"),
                CheckOutTime = reader.IsDBNull("CheckOutTime") ? (DateTime?)null : reader.GetDateTime("CheckOutTime"),
                CheckInPhoto = reader.IsDBNull("CheckInPhoto") ? null : Convert.ToBase64String((byte[])reader["CheckInPhoto"]),
                CheckOutPhoto = reader.IsDBNull("CheckOutPhoto") ? null : Convert.ToBase64String((byte[])reader["CheckOutPhoto"]),
                CheckInLatitude = reader.IsDBNull("CheckInLatitude") ? (double?)null : reader.GetDouble("CheckInLatitude"),
                CheckInLongitude = reader.IsDBNull("CheckInLongitude") ? (double?)null : reader.GetDouble("CheckInLongitude"),
                CheckOutLatitude = reader.IsDBNull("CheckOutLatitude") ? (double?)null : reader.GetDouble("CheckOutLatitude"),
                CheckOutLongitude = reader.IsDBNull("CheckOutLongitude") ? (double?)null : reader.GetDouble("CheckOutLongitude"),
                CheckInDevice = reader.IsDBNull("CheckInDevice") ? null : reader.GetString("CheckInDevice"),
                CheckOutDevice = reader.IsDBNull("CheckOutDevice") ? null : reader.GetString("CheckOutDevice")
            };

            results.Add(record);
        }

        if (results.Count == 0)
        {
            return NotFound("No check-in/check-out records found for this month.");
        }

        return Ok(results);
    }

}
