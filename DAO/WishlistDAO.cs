//using AppBackend.Objects;
//using Microsoft.Extensions.Configuration;
//using MySql.Data.MySqlClient;
//using System.Collections.Generic;
//using System.Data;
//using System.Threading.Tasks;

//namespace AppBackend.Data
//{
//    public class WishlistDAO
//    {
//        private readonly string _connectionString;

//        public WishlistDAO(IConfiguration configuration)
//        {
//            _connectionString = configuration.GetConnectionString("DefaultConnection");
//        }

//        public async Task<List<Wishlist>> GetEmployeesOfTheMonthAsync(int tenantId, int month, int year)
//        {
//            var query = @"
//                SELECT * 
//                FROM Employees 
//                WHERE 
//                    TenantID = @TenantID
//                    AND (
//                        (MONTH(DateOfBirth) = @CurrentMonth AND YEAR(DateOfBirth) = @CurrentYear)
//                        OR 
//                        (MONTH(DateOfJoining) = @CurrentMonth AND YEAR(DateOfJoining) = @CurrentYear)
//                    );
//            ";

//            var results = new List<Wishlist>();

//            using var connection = new MySqlConnection(_connectionString);
//            await connection.OpenAsync();

//            using var command = new MySqlCommand(query, connection);
//            command.Parameters.AddWithValue("@TenantID", tenantId);
//            command.Parameters.AddWithValue("@CurrentMonth", month);
//            command.Parameters.AddWithValue("@CurrentYear", year);

//            using var reader = await command.ExecuteReaderAsync();

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

//            return results;
//        }
//    }
//}
using AppBackend.Objects;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace AppBackend.Data
{
    public class WishlistDAO
    {
        private readonly string _connectionString;

        public WishlistDAO(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<EmployeesOfTheMonthResponse> GetEmployeesOfTheMonthAsync(int tenantId, int month)
        {
            var birthdayQuery = @"
                SELECT 
                    EmployeeID,
                    FirstName,
                    DateOfBirth
                FROM Employees 
                WHERE 
                    TenantID = @TenantID
                    AND MONTH(DateOfBirth) = @CurrentMonth;
                   
            ";

            var anniversaryQuery = @"
                SELECT 
                    EmployeeID,
                    FirstName,
                    DateOfJoining
                FROM Employees 
                WHERE 
                    TenantID = @TenantID
                    AND MONTH(DateOfJoining) = @CurrentMonth;
                    
            ";

            var birthdayResults = new List<Wishlist>();
            var anniversaryResults = new List<Wishlist>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Fetch birthdays
            using (var birthdayCommand = new MySqlCommand(birthdayQuery, connection))
            {
                birthdayCommand.Parameters.AddWithValue("@TenantID", tenantId);
                birthdayCommand.Parameters.AddWithValue("@CurrentMonth", month);

                using var birthdayReader = await birthdayCommand.ExecuteReaderAsync();
                while (await birthdayReader.ReadAsync())
                {
                    var record = new Wishlist
                    {
                        Id = birthdayReader.GetInt32("EmployeeID"),
                        Name = birthdayReader.GetString("FirstName"),
                        DateOfBirth = birthdayReader.GetDateTime("DateOfBirth"),
                        Status = "B-DAY"
                    };

                    birthdayResults.Add(record);
                }
            }

            // Fetch anniversaries
            using (var anniversaryCommand = new MySqlCommand(anniversaryQuery, connection))
            {
                anniversaryCommand.Parameters.AddWithValue("@TenantID", tenantId);
                anniversaryCommand.Parameters.AddWithValue("@CurrentMonth", month);

                using var anniversaryReader = await anniversaryCommand.ExecuteReaderAsync();
                while (await anniversaryReader.ReadAsync())
                {
                    var record = new Wishlist
                    {
                        Id = anniversaryReader.GetInt32("EmployeeID"),
                        Name = anniversaryReader.GetString("FirstName"),
                        DateOfJoining = anniversaryReader.GetDateTime("DateOfJoining"),
                        Status = "NEW"
                    };

                    anniversaryResults.Add(record);
                }
            }

            return new EmployeesOfTheMonthResponse
            {
                Birthdays = birthdayResults,
                Anniversaries = anniversaryResults
            };
        }
    }

    public class EmployeesOfTheMonthResponse
    {
        public List<Wishlist> Birthdays { get; set; }
        public List<Wishlist> Anniversaries { get; set; }
    }
}

