using SearchService.Models;

namespace SearchService.Repositories.Interfaces;

public interface IFlightInstanceRepository
{
    Task<FlightInstance?> GetByIdAsync(int instanceId);
    Task<FlightInstance?> GetByIdWithFlightAsync(int instanceId);
    Task AddAsync(FlightInstance instance);
    Task UpdateAsync(FlightInstance instance);
    Task SaveChangesAsync();
}
