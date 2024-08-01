using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Threading.Tasks;

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
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            if (request.RequestType == "checkin")
            {
                if (request.CheckInTime == null)
                {
                    return BadRequest("Check-in time is required.");
                }
                // Calculate the distance
                double distance = CalculateDistance(request.LocationLatitude, request.LocationLongitude, request.CheckInLatitude, request.CheckInLongitude);
                if (distance > 0.1) // Distance is in kilometers
                {
                    return BadRequest("Check-in location is more than 100 meters from the branch location.");
                }


                // Check for existing check-in for the same employee and date
                var checkExistingCheckInQuery = @"
                    SELECT COUNT(*)
                    FROM CheckInCheckOut
                    WHERE TenantID = @TenantID 
                      AND EmployeeID = @EmployeeID 
                      AND DATE(CheckInTime) = DATE(@CheckInTime) 
                      AND CheckOutTime IS NULL";

                using var checkExistingCommand = new MySqlCommand(checkExistingCheckInQuery, connection);
                checkExistingCommand.Parameters.AddWithValue("@TenantID", request.TenantID);
                checkExistingCommand.Parameters.AddWithValue("@EmployeeID", request.EmployeeID);
                checkExistingCommand.Parameters.AddWithValue("@CheckInTime", request.CheckInTime);

                var existingCheckInCount = Convert.ToInt32(await checkExistingCommand.ExecuteScalarAsync());

                if (existingCheckInCount > 0)
                {
                    return BadRequest("A check-in record already exists for the same date and has not been checked out.");
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

                using var checkInCommand = new MySqlCommand(checkInQuery, connection);
                checkInCommand.Parameters.AddWithValue("@TenantID", request.TenantID);
                checkInCommand.Parameters.AddWithValue("@EmployeeID", request.EmployeeID);
                checkInCommand.Parameters.AddWithValue("@CheckInTime", request.CheckInTime);
                checkInCommand.Parameters.AddWithValue("@CheckInPhoto", Convert.FromBase64String(request.CheckInPhoto));
                checkInCommand.Parameters.AddWithValue("@CheckInLatitude", request.CheckInLatitude);
                checkInCommand.Parameters.AddWithValue("@CheckInLongitude", request.CheckInLongitude);
                checkInCommand.Parameters.AddWithValue("@CheckInDevice", request.CheckInDevice);

                await checkInCommand.ExecuteNonQueryAsync();
                return Ok("Check-in recorded successfully.");
            }
            else if (request.RequestType == "checkout")
            {
                if (request.CheckOutTime == null)
                {
                    return BadRequest("Check-out time is required.");
                }
                double distance = CalculateDistance(request.LocationLatitude, request.LocationLongitude, (request.CheckOutLatitude), request.CheckOutLongitude);
                if (distance > 0.1)
                {
                    return BadRequest("Check-out location is more than 100 meters from the branch location.");
                }

                // Update the specific check-in record
                var checkOutQuery = @"
                    UPDATE CheckInCheckOut
                    SET CheckOutTime = @CheckOutTime,
                        CheckOutPhoto = @CheckOutPhoto,
                        CheckOutLatitude = @CheckOutLatitude,
                        CheckOutLongitude = @CheckOutLongitude,
                        CheckOutDevice = @CheckOutDevice
                    WHERE TenantID = @TenantID 
                      AND EmployeeID = @EmployeeID 
                      AND DATE(CheckInTime) = DATE(@CheckInTime) 
                      AND CheckOutTime IS NULL";

                using var checkOutCommand = new MySqlCommand(checkOutQuery, connection);
                checkOutCommand.Parameters.AddWithValue("@TenantID", request.TenantID);
                checkOutCommand.Parameters.AddWithValue("@EmployeeID", request.EmployeeID);
                checkOutCommand.Parameters.AddWithValue("@CheckInTime", request.CheckInTime);
                checkOutCommand.Parameters.AddWithValue("@CheckOutTime", request.CheckOutTime);
                checkOutCommand.Parameters.AddWithValue("@CheckOutPhoto", Convert.FromBase64String(request.CheckOutPhoto));
                checkOutCommand.Parameters.AddWithValue("@CheckOutLatitude", request.CheckOutLatitude);
                checkOutCommand.Parameters.AddWithValue("@CheckOutLongitude", request.CheckOutLongitude);
                checkOutCommand.Parameters.AddWithValue("@CheckOutDevice", request.CheckOutDevice);

                var rowsAffected = await checkOutCommand.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                {
                    return Ok();
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

    private double CalculateDistance(double lat1, double lon1, double? lat2, double? lon2)
    {
        if (lat2 == null || lon2 == null)
        {
            throw new ArgumentException("Latitude and longitude values cannot be null.");
        }

        const double R = 6371; // Radius of the Earth in kilometers
        var dLat = DegreesToRadians((double)lat2 - lat1);
        var dLon = DegreesToRadians((double)lon2 - lon1);
        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians((double)lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }


    private double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
