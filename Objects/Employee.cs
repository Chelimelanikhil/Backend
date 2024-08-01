namespace AppBackend.Objects
{
    public class Employee
    {
        public int? EmployeeID { get; set; }
        public string BranchName { get; set; }
        public int? DepartmentID { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfJoining { get; set; }
        public string WeekOff1 { get; set; }
        public string WeekOff2 { get; set; }
        public string JobTitle { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public string Status { get; set; }
        public int BranchId {  get; set; }   
        public int ShiftId { get; set; }
        public int ManagerId { get; set; }
        
    }

}
