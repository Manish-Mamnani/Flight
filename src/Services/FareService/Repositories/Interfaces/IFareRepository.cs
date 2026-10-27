using FareService.Models;

namespace FareService.Repositories.Interfaces;

public interface IFareRepository
{
    Task<Fare?> GetByIdAsync(int fareId);
    Task<IEnumerable<Fare>> GetByFlightInstanceIdAsync(int flightInstanceId);
    Task<Fare?> GetByInstanceAndClassAsync(int flightInstanceId, string fareClass);
    Task AddAsync(Fare fare);
    Task UpdateAsync(Fare fare);
    Task SaveChangesAsync();
}
