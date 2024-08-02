namespace AppBackend.Interfaces
{
    public interface ICheckinCheckoutDAO
    {
        Task<string> CheckInOrOutAsync(CheckinCheckout request);
        Task<string> AutomateCheckInCheckOutAsync();
    }


}
