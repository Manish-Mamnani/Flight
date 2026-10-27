using SearchService.DTOs;

namespace SearchService.Services.Interfaces;

public interface ISearchService
{
    Task<IEnumerable<FlightResponseDto>> SearchAsync(string origin, string destination, DateTime date);
    Task<FlightResponseDto> GetFlightAsync(int flightId);
    Task<FlightResponseDto> CreateFlightAsync(CreateFlightDto dto, int createdByUserId);
    Task<FlightResponseDto> UpdateFlightAsync(int flightId, UpdateFlightDto dto, int userId, bool isAdmin);
    Task<IEnumerable<FlightResponseDto>> GetMyFlightsAsync(int managerId);
    Task<IEnumerable<FlightResponseDto>> GetManagedFlightsAsync();
    Task<FlightValidationDto> ValidateInstanceAsync(int instanceId);
}
