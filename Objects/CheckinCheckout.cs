public class CheckinCheckout
{
    public int TenantID { get; set; }
    public int EmployeeID { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string? CheckInPhoto { get; set; } // Base64 encoded string
    public string? CheckOutPhoto { get; set; } // Base64 encoded string
    public double? CheckInLatitude { get; set; }
    public double? CheckInLongitude { get; set; }
    public double? CheckOutLatitude { get; set; }
    public double? CheckOutLongitude { get; set; }
    public string? CheckInDevice { get; set; }
    public string? CheckOutDevice { get; set; }
    public string RequestType { get; set; } // New property
    public float LocationLatitude { get; set; }
    public float LocationLongitude { get; set; }
}
