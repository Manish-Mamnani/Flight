using SearchService.Models;

namespace SearchService.Repositories.Interfaces;

public interface IAirlineRepository
{
    Task<Airline?> GetByIdAsync(int airlineId);
    Task<Airline?> GetByCodeAsync(string airlineCode);
    Task<IEnumerable<Airline>> GetAllActiveAsync();
    Task AddAsync(Airline airline);
    Task SaveChangesAsync();
}
