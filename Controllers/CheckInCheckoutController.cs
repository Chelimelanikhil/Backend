//using AppBackend.Objects;
//using Microsoft.AspNetCore.Mvc;
//using MySql.Data.MySqlClient;
//using System;
//using System.Data;
//using System.Threading.Tasks;
//using Twilio.Types;
//using Twilio;
//using Twilio;
//using Twilio.Rest.Api.V2010.Account;
//using Twilio.Types;

//[ApiController]
//[Route("[controller]")]
//public class CheckinCheckoutController : ControllerBase
//{
//    private readonly string _connectionString;

//    public CheckinCheckoutController(IConfiguration configuration)
//    {
//        _connectionString = configuration.GetConnectionString("DefaultConnection");
//    }

//    [HttpPost]
//    public async Task<IActionResult> CheckInOrOut([FromBody] CheckinCheckout request)
//    {
//        if (request == null)
//        {
//            return BadRequest("Request body is null.");
//        }

//        try
//        {
//            using var connection = new MySqlConnection(_connectionString);
//            await connection.OpenAsync();

//            if (request.RequestType == "checkin")
//            {
//                if (request.CheckInTime == null)
//                {
//                    return BadRequest("Check-in time is required.");
//                }
//                // Calculate the distance
//                double distance = CalculateDistance(request.LocationLatitude, request.LocationLongitude, request.CheckInLatitude, request.CheckInLongitude);
//                if (distance > 0.1) // Distance is in kilometers
//                {
//                    return BadRequest("Check-in location is more than 100 meters from the branch location.");
//                }


//                // Check for existing check-in for the same employee and date
//                var checkExistingCheckInQuery = @"
//                    SELECT COUNT(*)
//                    FROM CheckInCheckOut
//                    WHERE TenantID = @TenantID 
//                      AND EmployeeID = @EmployeeID 
//                      AND DATE(CheckInTime) = DATE(@CheckInTime) 
//                      AND CheckOutTime IS NULL";

//                using var checkExistingCommand = new MySqlCommand(checkExistingCheckInQuery, connection);
//                checkExistingCommand.Parameters.AddWithValue("@TenantID", request.TenantID);
//                checkExistingCommand.Parameters.AddWithValue("@EmployeeID", request.EmployeeID);
//                checkExistingCommand.Parameters.AddWithValue("@CheckInTime", request.CheckInTime);

//                var existingCheckInCount = Convert.ToInt32(await checkExistingCommand.ExecuteScalarAsync());

//                if (existingCheckInCount > 0)
//                {
//                    return BadRequest("A check-in record already exists for the same date and has not been checked out.");
//                }

//                var checkInQuery = @"
//                    INSERT INTO CheckInCheckOut (
//                        TenantID, EmployeeID, CheckInTime, CheckOutTime,
//                        CheckInPhoto, CheckOutPhoto, CheckInLatitude, CheckInLongitude,
//                        CheckOutLatitude, CheckOutLongitude, CheckInDevice, CheckOutDevice
//                    )
//                    VALUES (
//                        @TenantID, @EmployeeID, @CheckInTime, NULL,
//                        @CheckInPhoto, NULL, @CheckInLatitude, @CheckInLongitude,
//                        NULL, NULL, @CheckInDevice, NULL
//                    )";

//                using var checkInCommand = new MySqlCommand(checkInQuery, connection);
//                checkInCommand.Parameters.AddWithValue("@TenantID", request.TenantID);
//                checkInCommand.Parameters.AddWithValue("@EmployeeID", request.EmployeeID);
//                checkInCommand.Parameters.AddWithValue("@CheckInTime", request.CheckInTime);
//                checkInCommand.Parameters.AddWithValue("@CheckInPhoto", Convert.FromBase64String(request.CheckInPhoto));
//                checkInCommand.Parameters.AddWithValue("@CheckInLatitude", request.CheckInLatitude);
//                checkInCommand.Parameters.AddWithValue("@CheckInLongitude", request.CheckInLongitude);
//                checkInCommand.Parameters.AddWithValue("@CheckInDevice", request.CheckInDevice);

//                await checkInCommand.ExecuteNonQueryAsync();
//                return Ok("Check-in recorded successfully.");
//            }
//            else if (request.RequestType == "checkout")
//            {
//                if (request.CheckOutTime == null)
//                {
//                    return BadRequest("Check-out time is required.");
//                }
//                double distance = CalculateDistance(request.LocationLatitude, request.LocationLongitude, (request.CheckOutLatitude), request.CheckOutLongitude);
//                if (distance > 0.1)
//                {
//                    return BadRequest("Check-out location is more than 100 meters from the branch location.");
//                }

//                // Update the specific check-in record
//                var checkOutQuery = @"
//                    UPDATE CheckInCheckOut
//                    SET CheckOutTime = @CheckOutTime,
//                        CheckOutPhoto = @CheckOutPhoto,
//                        CheckOutLatitude = @CheckOutLatitude,
//                        CheckOutLongitude = @CheckOutLongitude,
//                        CheckOutDevice = @CheckOutDevice
//                    WHERE TenantID = @TenantID 
//                      AND EmployeeID = @EmployeeID 
//                      AND DATE(CheckInTime) = DATE(@CheckInTime) 
//                      AND CheckOutTime IS NULL";

//                using var checkOutCommand = new MySqlCommand(checkOutQuery, connection);
//                checkOutCommand.Parameters.AddWithValue("@TenantID", request.TenantID);
//                checkOutCommand.Parameters.AddWithValue("@EmployeeID", request.EmployeeID);
//                checkOutCommand.Parameters.AddWithValue("@CheckInTime", request.CheckInTime);
//                checkOutCommand.Parameters.AddWithValue("@CheckOutTime", request.CheckOutTime);
//                checkOutCommand.Parameters.AddWithValue("@CheckOutPhoto", Convert.FromBase64String(request.CheckOutPhoto));
//                checkOutCommand.Parameters.AddWithValue("@CheckOutLatitude", request.CheckOutLatitude);
//                checkOutCommand.Parameters.AddWithValue("@CheckOutLongitude", request.CheckOutLongitude);
//                checkOutCommand.Parameters.AddWithValue("@CheckOutDevice", request.CheckOutDevice);

//                var rowsAffected = await checkOutCommand.ExecuteNonQueryAsync();
//                if (rowsAffected > 0)
//                {
//                    return Ok("Check-out recorded successfully.");
//                }
//                else
//                {
//                    return NotFound("No matching check-in record found to update.");
//                }
//            }
//            else
//            {
//                return BadRequest("Invalid request type.");
//            }
//        }
//        catch (Exception ex)
//        {
//            // Log exception here (optional)
//            return StatusCode(500, $"Internal server error: {ex.Message}");
//        }
//    }

//    private double CalculateDistance(double lat1, double lon1, double? lat2, double? lon2)
//    {
//        if (lat2 == null || lon2 == null)
//        {
//            throw new ArgumentException("Latitude and longitude values cannot be null.");
//        }

//        const double R = 6371; // Radius of the Earth in kilometers
//        var dLat = DegreesToRadians((double)lat2 - lat1);
//        var dLon = DegreesToRadians((double)lon2 - lon1);
//        var a =
//            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
//            Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians((double)lat2)) *
//            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
//        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
//        return R * c;
//    }
//    [HttpPost]
//    [Route("automate-checkin-checkout")]
//    public async Task<IActionResult> AutomateCheckInCheckOut()
//    {

//        //RECOVERY CODE - Z2DXUAH4YLPTQLR1L1WYCCA8
//        try
//        {
//            using var connection = new MySqlConnection(_connectionString);
//            await connection.OpenAsync();

//            // Get all holidays
//            var holidays = new List<Holiday>();
//            var holidaysQuery = "SELECT * FROM holiday WHERE TenantID = @TenantID";
//            using var holidaysCommand = new MySqlCommand(holidaysQuery, connection);
//            holidaysCommand.Parameters.AddWithValue("@TenantID", 1);

//            using (var reader = await holidaysCommand.ExecuteReaderAsync())
//            {
//                while (await reader.ReadAsync())
//                {
//                    holidays.Add(new Holiday
//                    {
//                        HolidayID = reader.GetInt32("HolidayID"),
//                        TenantID = reader.GetInt32("TenantID"),
//                        startDate = reader.GetDateTime("HolidayStartDate"),
//                        endDate = reader.GetDateTime("HolidayEndDate"),
//                    });
//                }
//            }

//            // Get all employees
//            var employees = new List<Employee>();
//            var employeesQuery = @"
//            SELECT 
//                e.*, 
//                b.LocationLatitude, b.LocationLongitude, 
//                s.shiftStartingTime, s.shiftEndingTime 
//            FROM Employees e
//            INNER JOIN Branches b ON e.BranchId = b.BranchId
//            INNER JOIN Shifts s ON e.ShiftID = s.shiftId
//            WHERE e.TenantID = @TenantID";

//            using var employeesCommand = new MySqlCommand(employeesQuery, connection);
//            employeesCommand.Parameters.AddWithValue("@TenantID", 1);

//            using (var reader = await employeesCommand.ExecuteReaderAsync())
//            {
//                while (await reader.ReadAsync())
//                {
//                    employees.Add(new Employee
//                    {
//                        EmployeeID = reader.GetInt32("EmployeeID"),
//                        TenantID = reader.GetInt32("TenantID"),
//                        DepartmentID = reader.GetInt32("DepartmentID"),
//                        FirstName = reader.GetString("FirstName"),
//                        LastName = reader.GetString("LastName"),
//                        Email = reader.GetString("Email"),
//                        PhoneNumber = reader.GetString("PhoneNumber"),
//                        DateOfBirth = reader.GetDateTime("DateOfBirth"),
//                        DateOfJoining = reader.GetDateTime("DateOfJoining"),
//                        WeekOff1 = reader.GetString("WeekOff1"),
//                        WeekOff2 = reader.GetString("WeekOff2"),
//                        JobTitle = reader.GetString("JobTitle"),
//                        Salary = reader.GetDecimal("Salary"),
//                        Address = reader.GetString("Address"),
//                        City = reader.GetString("City"),
//                        State = reader.GetString("State"),
//                        ZipCode = reader.GetString("ZipCode"),
//                        Country = reader.GetString("Country"),
//                        Status = reader.GetString("Status"),
//                        BranchId = reader.GetInt32("BranchId"),
//                        ShiftId = reader.GetInt32("ShiftID")
//                    });
//                }
//            }

//            var currentDate = DateTime.UtcNow.Date;

//            // Process each employee
//            foreach (var employee in employees)
//            {
//                // Fetch branch details
//                var branchQuery = @"
//                SELECT 
//                    LocationLatitude,
//                    LocationLongitude
//                FROM 
//                    Branches
//                WHERE 
//                    BranchId = @BranchId";

//                double locationLatitude;
//                double locationLongitude;
//                using (var branchCommand = new MySqlCommand(branchQuery, connection))
//                {
//                    branchCommand.Parameters.AddWithValue("@BranchId", employee.BranchId);
//                    using (var branchReader = await branchCommand.ExecuteReaderAsync())
//                    {
//                        if (await branchReader.ReadAsync())
//                        {
//                            locationLatitude = branchReader.GetDouble("LocationLatitude");
//                            locationLongitude = branchReader.GetDouble("LocationLongitude");
//                        }
//                        else
//                        {
//                            continue; // Skip this employee if branch data is not available
//                        }
//                    }
//                }

//                // Fetch shift details
//                var shiftQuery = @"
//                SELECT 
//                    shiftStartingTime,
//                    shiftEndingTime
//                FROM 
//                    shifts
//                WHERE 
//                    shiftId = @ShiftId";

//                var shiftStartingTime = TimeSpan.Zero; // Default value
//                var shiftEndingTime = TimeSpan.Zero;   // Default value
//                using (var shiftCommand = new MySqlCommand(shiftQuery, connection))
//                {
//                    shiftCommand.Parameters.AddWithValue("@ShiftId", employee.ShiftId);

//                    using (var shiftReader = await shiftCommand.ExecuteReaderAsync())
//                    {

//                        if (await shiftReader.ReadAsync())
//                        {
//                            // Read as TimeSpan directly
//                            shiftStartingTime = (TimeSpan)shiftReader["shiftStartingTime"];
//                            shiftEndingTime = (TimeSpan)shiftReader["shiftEndingTime"];
//                        }

//                        else
//                        {
//                            continue; // Skip this employee if shift data is not available
//                        }
//                    }
//                }

//                var isHoliday = holidays.Any(h => currentDate >= h.startDate && currentDate <= h.endDate);
//                var isWeekOff = currentDate.DayOfWeek.ToString() == employee.WeekOff1 || currentDate.DayOfWeek.ToString() == employee.WeekOff2;

//                if (isHoliday || isWeekOff)
//                {
//                    // Insert check-in/check-out record with 00:00:00 times
//                    var insertQuery = @"
//                    INSERT INTO CheckInCheckOut (
//                        TenantID, EmployeeID, CheckInTime, CheckOutTime, 
//                        CheckInPhoto, CheckOutPhoto, CheckInLatitude, CheckInLongitude, 
//                        CheckOutLatitude, CheckOutLongitude, CheckInDevice, CheckOutDevice
//                    ) VALUES (
//                        @TenantID, @EmployeeID, @CheckInTime, @CheckOutTime, 
//                        NULL, NULL, @CheckInLatitude, @CheckInLongitude, 
//                        @CheckOutLatitude, @CheckOutLongitude, NULL, NULL
//                    )";

//                    using var insertCommand = new MySqlCommand(insertQuery, connection);
//                    insertCommand.Parameters.AddWithValue("@TenantID", employee.TenantID);
//                    insertCommand.Parameters.AddWithValue("@EmployeeID", employee.EmployeeID);
//                    insertCommand.Parameters.AddWithValue("@CheckInTime", new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 0, 0, 0));
//                    insertCommand.Parameters.AddWithValue("@CheckOutTime", new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 0, 0, 0));
//                    insertCommand.Parameters.AddWithValue("@CheckInLatitude", locationLatitude);
//                    insertCommand.Parameters.AddWithValue("@CheckInLongitude", locationLongitude);
//                    insertCommand.Parameters.AddWithValue("@CheckOutLatitude", locationLatitude);
//                    insertCommand.Parameters.AddWithValue("@CheckOutLongitude", locationLongitude);

//                    await insertCommand.ExecuteNonQueryAsync();
//                }
//                else
//                {
//                    // Check for late check-ins or missed check-ins
//                    var checkInQuery = @"
//                    SELECT CheckInTime 
//                    FROM CheckInCheckOut 
//                    WHERE TenantID = @TenantID 
//                      AND EmployeeID = @EmployeeID 
//                      AND DATE(CheckInTime) = @CurrentDate";

//                    using var checkInCommand = new MySqlCommand(checkInQuery, connection);
//                    checkInCommand.Parameters.AddWithValue("@TenantID", employee.TenantID);
//                    checkInCommand.Parameters.AddWithValue("@EmployeeID", employee.EmployeeID);
//                    checkInCommand.Parameters.AddWithValue("@CurrentDate", currentDate);

//                    var checkInTime = (DateTime?)await checkInCommand.ExecuteScalarAsync();

//                    if (checkInTime == null)
//                    {
//                        // Send missed check-in notification
//                        SendNotification(employee.PhoneNumber, "Hi" + employee.LastName + employee.FirstName
//                        + "You have missed your check-in today.");
//                    }
//                    else if ((checkInTime.Value - currentDate.Add(shiftStartingTime)).TotalMinutes > 10)
//                    {
//                        var lateBy = checkInTime.Value - currentDate.Add(shiftStartingTime);
//                        var hoursLate = (int)lateBy.TotalHours;
//                        var minutesLate = lateBy.Minutes;
//                        SendNotification(employee.PhoneNumber, $"You have checked in late by {hoursLate} hours and {minutesLate} minutes.");
//                    }
//                }
//            }

//            return Ok("Check-in/check-out process completed successfully.");
//        }
//        catch (Exception ex)
//        {
//            return StatusCode(500, $"Internal server error: {ex.Message}");
//        }
//    }


//    private void SendNotification(string phoneNumber, string messageBody)
//    {
//        // Your Account SID and Auth Token from twilio.com/console
//        const string accountSid = "AC2d43a57d7bfea5f68722854a610ff4c7";
//        const string authToken = "c2e53d882cc92cd5b71f3b6c9ce9ea48";

//        // Initialize the Twilio client
//        TwilioClient.Init(accountSid, authToken);

//        // Define the message parameters
//        var to = new PhoneNumber(phoneNumber); // The phone number you want to send the message to
//        var from = new PhoneNumber("+17074665210"); // Your Twilio phone number

//        // Send the message
//        var message = MessageResource.Create(
//            to: to,
//            from: from,
//            body: messageBody);

//        // Output the message SID (optional)
//        Console.WriteLine($"Message SID: {message.Sid}");
//    }
//    private double DegreesToRadians(double degrees)
//    {
//        return degrees * Math.PI / 180;
//    }
//}



using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using AppBackend.Interfaces;
using AppBackend.DAO;

[ApiController]
[Route("[controller]")]
public class CheckinCheckoutController : ControllerBase
{
    private readonly ICheckinCheckoutDAO _checkinCheckoutDAO;

    public CheckinCheckoutController(ICheckinCheckoutDAO CheckinCheckoutDAO)
    {
        _checkinCheckoutDAO = CheckinCheckoutDAO;
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
            var result = await _checkinCheckoutDAO.CheckInOrOutAsync(request);
            if (result != null)
            {
<<<<<<< HEAD
                return Ok(result);
=======
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
>>>>>>> main
            }
            return BadRequest("Invalid request type.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost]
    [Route("automate-checkin-checkout")]
    public async Task<IActionResult> AutomateCheckInCheckOut()
    {
        try
        {
            var result = await _checkinCheckoutDAO.AutomateCheckInCheckOutAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}








