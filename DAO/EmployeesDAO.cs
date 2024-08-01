using AppBackend.Objects;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class EmployeeDAO
{
    private readonly string _connectionString;

    public EmployeeDAO(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<Employee>> GetEmployeeDetailsAsync(int employeeId, int tenantId)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            SELECT e.EmployeeID, d.DepartmentName AS BranchName, e.DepartmentID, e.FirstName, e.LastName, e.Email, 
                e.PhoneNumber, e.DateOfBirth, e.DateOfJoining, e.WeekOff1, e.WeekOff2, e.JobTitle, 
                e.Address, e.City, e.State, e.ZipCode, e.Country, e.Status, e.BranchID, e.shiftId, e.ManagerId
            FROM Employees e
            JOIN Departments d ON e.DepartmentID = d.DepartmentID
            WHERE e.EmployeeID = @EmployeeID AND e.TenantID = @TenantID";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@EmployeeID", employeeId);
        command.Parameters.AddWithValue("@TenantID", tenantId);

        var employees = new List<Employee>();

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var employee = new Employee
            {
                EmployeeID = reader.IsDBNull("EmployeeID") ? (int?)null : reader.GetInt32("EmployeeID"),
                DepartmentID = reader.IsDBNull("DepartmentID") ? (int?)null : reader.GetInt32("DepartmentID"),

                BranchName = reader.IsDBNull("BranchName") ? null : reader.GetString("BranchName"),
                FirstName = reader.GetString("FirstName"),
                LastName = reader.GetString("LastName"),
                Email = reader.GetString("Email"),
                PhoneNumber = reader.IsDBNull("PhoneNumber") ? null : reader.GetString("PhoneNumber"),
                DateOfBirth = reader.GetDateTime("DateOfBirth"),
                DateOfJoining = reader.GetDateTime("DateOfJoining"),
                WeekOff1 = reader.IsDBNull("WeekOff1") ? null : reader.GetString("WeekOff1"),
                WeekOff2 = reader.IsDBNull("WeekOff2") ? null : reader.GetString("WeekOff2"),
                JobTitle = reader.IsDBNull("JobTitle") ? null : reader.GetString("JobTitle"),
                
                Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                City = reader.IsDBNull("City") ? null : reader.GetString("City"),
                State = reader.IsDBNull("State") ? null : reader.GetString("State"),
                ZipCode = reader.IsDBNull("ZipCode") ? null : reader.GetString("ZipCode"),
                Country = reader.IsDBNull("Country") ? null : reader.GetString("Country"),
                Status = reader.GetString("Status"),
                BranchId = reader.GetInt32("BranchID"),
                ShiftId = reader.GetInt32("shiftId"),
                ManagerId = reader.GetInt32("ManagerId")

            };
            employees.Add(employee);
        }

        return employees;
    }
    public async Task<Employee> GetIndividualEmployeeDetails(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
        SELECT e.EmployeeID, d.DepartmentName AS BranchName, e.DepartmentID, e.FirstName, e.LastName, e.Email, 
            e.PhoneNumber, e.DateOfBirth, e.DateOfJoining, e.WeekOff1, e.WeekOff2, e.JobTitle, 
            e.Address, e.City, e.State, e.ZipCode, e.Country, e.Status, e.BranchID, e.shiftId, e.ManagerId
        FROM Employees e
        JOIN Departments d ON e.DepartmentID = d.DepartmentID
        WHERE e.EmployeeID = @Id";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            var employee = new Employee
            {
                EmployeeID = reader.IsDBNull("EmployeeID") ? (int?)null : reader.GetInt32("EmployeeID"),
                DepartmentID = reader.IsDBNull("DepartmentID") ? (int?)null : reader.GetInt32("DepartmentID"),
                BranchName = reader.IsDBNull("BranchName") ? null : reader.GetString("BranchName"),
                FirstName = reader.GetString("FirstName"),
                LastName = reader.GetString("LastName"),
                Email = reader.GetString("Email"),
                PhoneNumber = reader.IsDBNull("PhoneNumber") ? null : reader.GetString("PhoneNumber"),
                DateOfBirth = reader.IsDBNull("DateOfBirth") ? (DateTime?)null : reader.GetDateTime("DateOfBirth"),
                DateOfJoining = reader.IsDBNull("DateOfJoining") ? (DateTime?)null : reader.GetDateTime("DateOfJoining"),
                WeekOff1 = reader.IsDBNull("WeekOff1") ? null : reader.GetString("WeekOff1"),
                WeekOff2 = reader.IsDBNull("WeekOff2") ? null : reader.GetString("WeekOff2"),
                JobTitle = reader.IsDBNull("JobTitle") ? null : reader.GetString("JobTitle"),
                Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                City = reader.IsDBNull("City") ? null : reader.GetString("City"),
                State = reader.IsDBNull("State") ? null : reader.GetString("State"),
                ZipCode = reader.IsDBNull("ZipCode") ? null : reader.GetString("ZipCode"),
                Country = reader.IsDBNull("Country") ? null : reader.GetString("Country"),
                Status = reader.GetString("Status"),
                BranchId = reader.GetInt32("BranchID"),
                ShiftId = reader.GetInt32("shiftId"),
                ManagerId = reader.GetInt32("ManagerId")
            };

            return employee;
        }

        return null; // Return null if no employee found with the given ID
    }

}
