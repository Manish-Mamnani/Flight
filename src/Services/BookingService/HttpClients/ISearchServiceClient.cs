using BookingService.DTOs;

namespace BookingService.HttpClients;

public interface ISearchServiceClient
{
    Task<FlightValidationResultDto?> ValidateFlightInstanceAsync(int instanceId);
}
