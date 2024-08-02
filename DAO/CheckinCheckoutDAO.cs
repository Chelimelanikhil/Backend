using System;
using System.Threading.Tasks;
using AppBackend.Objects;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace AppBackend.DAO
{
    public class CheckinCheckoutDAO
    {
        private readonly string _connectionString;

        public CheckinCheckoutDAO(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<string> CheckInOrOutAsync(CheckinCheckout request)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            if (request.RequestType == "checkin")
            {
                if (request.CheckInTime == null)
                {
                    throw new ArgumentException("Check-in time is required.");
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
                checkInCommand.Parameters.AddWithValue("@CheckInTime", DateTime.UtcNow);
                checkInCommand.Parameters.AddWithValue("@CheckInPhoto", Convert.FromBase64String(request.CheckInPhoto));
                checkInCommand.Parameters.AddWithValue("@CheckInLatitude", request.CheckInLatitude);
                checkInCommand.Parameters.AddWithValue("@CheckInLongitude", request.CheckInLongitude);
                checkInCommand.Parameters.AddWithValue("@CheckInDevice", request.CheckInDevice);

                await checkInCommand.ExecuteNonQueryAsync();
                return "Check-in recorded successfully.";
            }
            else if (request.RequestType == "checkout")
            {
                if (request.CheckOutTime == null)
                {
                    throw new ArgumentException("Check-out time is required.");
                }

                var shiftQuery = @"
                SELECT 
                    shiftStartingTime,
                    shiftEndingTime
                FROM 
                    shifts
                WHERE 
                    shiftId = @ShiftId";

                var shiftStartingTime = TimeSpan.Zero;
                var shiftEndingTime = TimeSpan.Zero;
                using (var shiftCommand = new MySqlCommand(shiftQuery, connection))
                {
                    shiftCommand.Parameters.AddWithValue("@ShiftId", 1);

                    using (var shiftReader = await shiftCommand.ExecuteReaderAsync())
                    {
                        if (await shiftReader.ReadAsync())
                        {
                            shiftStartingTime = (TimeSpan)shiftReader["shiftStartingTime"];
                            shiftEndingTime = (TimeSpan)shiftReader["shiftEndingTime"];
                        }
                    }
                }

                var checkOutQuery = @"
                UPDATE CheckInCheckOut
                SET CheckOutTime = @CheckOutTime,
                    CheckOutPhoto = @CheckOutPhoto,
                    CheckOutLatitude = @CheckOutLatitude,
                    CheckOutLongitude = @CheckOutLongitude,
                    CheckOutDevice = @CheckOutDevice,
                    DayMark=@DayMark,
                    DayFrac = @DayFrac,
                    PaidFrac=@PaidFrac
                WHERE TenantID = @TenantID 
                  AND EmployeeID = @EmployeeID 
                  AND DATE(CheckInTime) = DATE(@CheckInTime) 
                  AND CheckOutTime IS NULL";

                var DayMark = "";
                var DayFrac = 0.0;
                var PaidFrac = 0.0;

                var totalHoursWorked = (request.CheckOutTime.Value - request.CheckInTime.Value).TotalHours;

                if (totalHoursWorked >= (shiftEndingTime - shiftStartingTime).TotalHours - 1)
                {
                    DayMark = "PP";
                    DayFrac = 1;
                    PaidFrac = 1;
                }
                else if (totalHoursWorked < (shiftEndingTime - shiftStartingTime).TotalHours / 2)
                {
                    DayMark = "P*A";
                    DayFrac = 0.5;
                    PaidFrac = 0.5;
                }
                else
                {
                    DayMark = "AA";
                    DayFrac = 0;
                    PaidFrac = 0;
                }

                using var checkOutCommand = new MySqlCommand(checkOutQuery, connection);
                checkOutCommand.Parameters.AddWithValue("@TenantID", request.TenantID);
                checkOutCommand.Parameters.AddWithValue("@EmployeeID", request.EmployeeID);
                checkOutCommand.Parameters.AddWithValue("@CheckInTime", request.CheckInTime);
                checkOutCommand.Parameters.AddWithValue("@CheckOutTime", DateTime.UtcNow);
                checkOutCommand.Parameters.AddWithValue("@CheckOutPhoto", Convert.FromBase64String(request.CheckOutPhoto));
                checkOutCommand.Parameters.AddWithValue("@CheckOutLatitude", request.CheckOutLatitude);
                checkOutCommand.Parameters.AddWithValue("@CheckOutLongitude", request.CheckOutLongitude);
                checkOutCommand.Parameters.AddWithValue("@CheckOutDevice", request.CheckOutDevice);
                checkOutCommand.Parameters.AddWithValue("@DayMark", DayMark);
                checkOutCommand.Parameters.AddWithValue("@DayFrac", DayFrac);
                checkOutCommand.Parameters.AddWithValue("@PaidFrac", PaidFrac);

                var rowsAffected = await checkOutCommand.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                {
                    return "Check-out recorded successfully.";
                }
                else
                {
                    throw new ArgumentException("No matching check-in record found to update.");
                }
            }
            else
            {
                throw new ArgumentException("Invalid request type.");
            }
        }
    }
}
