namespace AppBackend.Objects
{
    public class Holiday
    {
        public int HolidayID { get; set; }
        public int TenantID { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
     
    }

}
