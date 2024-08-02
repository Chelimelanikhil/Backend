using AppBackend.DAO;
using AppBackend.Objects;


namespace AppBackend.Manager
{
    public class CheckinCheckoutMgr
    {
        private readonly IConfiguration _configuration;

        public CheckinCheckoutMgr(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> HandleCheckinAsync(CheckinCheckout request)
        {
            if (request.CheckInTime == null)
            {
                throw new ArgumentException("Check-in time is required.");
            }

            // Calculate the distance
            double distance = CalculateDistance(request.LocationLatitude, request.LocationLongitude, request.CheckInLatitude, request.CheckInLongitude);
            if (distance > 0.1) // Distance is in kilometers
            {
                throw new ArgumentException("Check-in location is more than 100 meters from the branch location.");
            }

            CheckinCheckoutDAO checkinCheckoutDAO = new CheckinCheckoutDAO(_configuration);
            await checkinCheckoutDAO.CheckInOrOutAsync(request);
            return "Check-in recorded successfully.";
        }

        public async Task<string> HandleCheckoutAsync(CheckinCheckout request)
        {
            if (request.CheckOutTime == null)
            {
                throw new ArgumentException("Check-in time is required.");
            }

            // Calculate the distance
            double distance = CalculateDistance(request.LocationLatitude, request.LocationLongitude, request.CheckInLatitude, request.CheckInLongitude);
            if (distance > 0.1) // Distance is in kilometers
            {
                return ("Check-in location is more than 100 meters from the branch location.");
            }

            CheckinCheckoutDAO checkinCheckoutDAO = new CheckinCheckoutDAO(_configuration);
            await checkinCheckoutDAO.CheckInOrOutAsync(request);
            return "Check-in recorded successfully.";
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
            return degrees * (Math.PI / 180);
        }
    }
}
