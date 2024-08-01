using AppBackend.Objects;


namespace AppBackend.Manager
{
    public class EmployeeMgr
    {
        private readonly IConfiguration _configuration;

        public EmployeeMgr(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<List<Employee>> GetEmployeeDetailsAsync(int employeeId, int tenantId)
        {
            EmployeeDAO employeeDAO = new EmployeeDAO(_configuration);
            return employeeDAO.GetEmployeeDetailsAsync(employeeId, tenantId);
        }
        public Task<Employee> GetIndividualEmployeeDetails(int id)
        {
            EmployeeDAO employeeDAO = new EmployeeDAO(_configuration);
            return employeeDAO.GetIndividualEmployeeDetails(id);
        }
    }
}
