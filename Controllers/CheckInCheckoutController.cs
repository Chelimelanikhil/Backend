using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

[ApiController]
[Route("[controller]")]
public class CheckinCheckoutController : ControllerBase
{
    private readonly string _connectionString;

    public CheckinCheckoutController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    [HttpPost]
    public async Task<IActionResult> CheckInOrOut([FromBody] CheckinCheckout request)
    {
        if (request == null)
        {
            return BadRequest("Request body is null.");
        }

        try
        {
            if (request.RequestType == "checkin")
            {
                // Check-in request
                if (request.CheckInTime == null)
                {
                    return BadRequest("Check-in time is required.");
                }

                var checkInQuery = @"
                    INSERT INTO CheckInCheckOut (
                        TenantID, EmployeeID, CheckInTime, CheckOutTime,
                        CheckInPhoto, CheckOutPhoto, CheckInLatitude, CheckInLongitude,
                        CheckOutLatitude, CheckOutLongitude, CheckInDevice, CheckOutDevice
                    )
                    VALUES (
                        @TenantID, @EmployeeID, @CheckInTime, NULL,
                        @CheckInPhoto, NULL, @CheckInLatitude, @CheckInLongitude,
                        NULL, NULL, @CheckInDevice, NULL
                    )";

                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand(checkInQuery, connection);
                command.Parameters.AddWithValue("@TenantID", request.TenantID);
                command.Parameters.AddWithValue("@EmployeeID", request.EmployeeID);
                command.Parameters.AddWithValue("@CheckInTime", request.CheckInTime);
                command.Parameters.AddWithValue("@CheckInPhoto", Convert.FromBase64String(request.CheckInPhoto));
                command.Parameters.AddWithValue("@CheckInLatitude", request.CheckInLatitude);
                command.Parameters.AddWithValue("@CheckInLongitude", request.CheckInLongitude);
                command.Parameters.AddWithValue("@CheckInDevice", request.CheckInDevice);

                await command.ExecuteNonQueryAsync();
                return Ok("Check-in recorded successfully.");
            }
            else if (request.RequestType == "checkout")
            {
                // Check-out request
                if (request.CheckOutTime == null)
                {
                    return BadRequest("Check-out time is required.");
                }

                var checkOutQuery = @"
                    UPDATE CheckInCheckOut
                    SET CheckOutTime = @CheckOutTime,
                        CheckOutPhoto = @CheckOutPhoto,
                        CheckOutLatitude = @CheckOutLatitude,
                        CheckOutLongitude = @CheckOutLongitude,
                        CheckOutDevice = @CheckOutDevice
                    WHERE TenantID = @TenantID AND EmployeeID = @EmployeeID AND CheckOutTime IS NULL";

                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand(checkOutQuery, connection);
                command.Parameters.AddWithValue("@TenantID", request.TenantID);
                command.Parameters.AddWithValue("@EmployeeID", request.EmployeeID);
                command.Parameters.AddWithValue("@CheckOutTime", request.CheckOutTime);
                command.Parameters.AddWithValue("@CheckOutPhoto", Convert.FromBase64String(request.CheckOutPhoto));
                command.Parameters.AddWithValue("@CheckOutLatitude", request.CheckOutLatitude);
                command.Parameters.AddWithValue("@CheckOutLongitude", request.CheckOutLongitude);
                command.Parameters.AddWithValue("@CheckOutDevice", request.CheckOutDevice);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                {
                    return Ok("Check-out recorded successfully.");
                }
                else
                {
                    return NotFound("No matching check-in record found to update.");
                }
            }
            else
            {
                return BadRequest("Invalid request type.");
            }
        }
        catch (Exception ex)
        {
            // Log exception here (optional)
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
