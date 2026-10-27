using SearchService.Models;

namespace SearchService.Repositories.Interfaces;

public interface IAirportRepository
{
    Task<Airport?> GetByIdAsync(int airportId);
    Task<Airport?> GetByCodeAsync(string airportCode);
    Task<IEnumerable<Airport>> GetAllActiveAsync();
    Task AddAsync(Airport airport);
    Task UpdateAsync(Airport airport);
    Task SaveChangesAsync();
}
