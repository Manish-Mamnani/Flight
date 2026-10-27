using FareService.DTOs;

namespace FareService.HttpClients;

public interface ISearchServiceClient
{
    Task<FlightValidationResponseDto?> ValidateFlightInstanceAsync(int instanceId);
}
