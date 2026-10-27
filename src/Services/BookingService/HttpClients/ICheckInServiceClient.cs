namespace BookingService.HttpClients;

public interface ICheckInServiceClient
{
    Task<bool> ReleaseSeatsAsync(int bookingId);
}
