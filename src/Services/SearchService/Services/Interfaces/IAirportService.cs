using SearchService.DTOs;

namespace SearchService.Services.Interfaces;

public interface IAirportService
{
    Task<IEnumerable<AirportDto>> GetAirportsAsync();
    Task<AirportDto> CreateAirportAsync(CreateAirportDto dto);
    Task<AirportDto> UpdateAirportAsync(int id, UpdateAirportDto dto);
}
