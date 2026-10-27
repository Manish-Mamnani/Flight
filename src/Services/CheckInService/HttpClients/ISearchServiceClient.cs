using CheckInService.DTOs;

namespace CheckInService.HttpClients;

public interface ISearchServiceClient
{
    Task<FlightDetailsResponseDto?> GetFlightInstanceAsync(int instanceId);
}
