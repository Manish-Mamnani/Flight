using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Models;
using SearchService.Repositories.Interfaces;

namespace SearchService.Repositories;

public class FlightInstanceRepository : IFlightInstanceRepository
{
    private readonly SearchDbContext _context;

    public FlightInstanceRepository(SearchDbContext context)
    {
        _context = context;
    }

    public async Task<FlightInstance?> GetByIdAsync(int instanceId)
    {
        return await _context.FlightInstances.FindAsync(instanceId);
    }

    public async Task<FlightInstance?> GetByIdWithFlightAsync(int instanceId)
    {
        return await _context.FlightInstances
            .Include(fi => fi.Flight)
            .ThenInclude(f => f.Airline)
            .Include(fi => fi.Flight)
            .ThenInclude(f => f.OriginAirport)
            .Include(fi => fi.Flight)
            .ThenInclude(f => f.DestinationAirport)
            .FirstOrDefaultAsync(fi => fi.FlightInstanceId == instanceId);
    }

    public async Task AddAsync(FlightInstance instance)
    {
        await _context.FlightInstances.AddAsync(instance);
    }

    public Task UpdateAsync(FlightInstance instance)
    {
        _context.FlightInstances.Update(instance);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
