//using AppBackend.Data;
//using AppBackend.Objects;

//namespace AppBackend.Manager
//{   
//    public class WishlistMgr
//    {
//        private readonly IConfiguration _configuration;

//        public WishlistMgr(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        public Task<List<Wishlist>> GetEmployeesOfTheMonthAsync(int tenantId, int month, int year)
//        {
//            WishlistDAO wishlistDAO = new WishlistDAO(_configuration);
//            return wishlistDAO.GetEmployeesOfTheMonthAsync(tenantId, month, year);
//        }
//    }
//}

using AppBackend.Data;
using AppBackend.Objects;
using System.Threading.Tasks;

namespace AppBackend.Manager
{
    public class WishlistMgr
    {
        private readonly IConfiguration _configuration;

        public WishlistMgr(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<EmployeesOfTheMonthResponse> GetEmployeesOfTheMonthAsync(int tenantId, int month)
        {
            WishlistDAO wishlistDAO = new WishlistDAO(_configuration);
            return wishlistDAO.GetEmployeesOfTheMonthAsync(tenantId, month);
        }
    }
}

