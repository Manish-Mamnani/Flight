using SearchService.Models;

namespace SearchService.Repositories.Interfaces;

public interface IFlightRepository
{
    Task<Flight?> GetByIdAsync(int flightId);
    Task<Flight?> GetByIdWithDetailsAsync(int flightId);
    Task<IEnumerable<Flight>> SearchFlightsAsync(string originCode, string destinationCode, DateTime departureDate);
    Task<IEnumerable<Flight>> GetByManagerIdAsync(int managerId);
    Task<IEnumerable<Flight>> GetAllManagedAsync();
    Task AddAsync(Flight flight);
    Task UpdateAsync(Flight flight);
    Task SaveChangesAsync();
}
