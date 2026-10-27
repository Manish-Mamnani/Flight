using FareService.Data;
using FareService.Models;
using FareService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FareService.Repositories;

public class FareRepository : IFareRepository
{
    private readonly FareDbContext _context;

    public FareRepository(FareDbContext context)
    {
        _context = context;
    }

    public async Task<Fare?> GetByIdAsync(int fareId)
    {
        return await _context.Fares.FindAsync(fareId);
    }

    public async Task<IEnumerable<Fare>> GetByFlightInstanceIdAsync(int flightInstanceId)
    {
        return await _context.Fares
            .Where(f => f.FlightInstanceId == flightInstanceId && f.Status == "Active")
            .ToListAsync();
    }

    public async Task<Fare?> GetByInstanceAndClassAsync(int flightInstanceId, string fareClass)
    {
        var normalized = fareClass.Trim();
        return await _context.Fares
            .FirstOrDefaultAsync(f => f.FlightInstanceId == flightInstanceId && f.FareClass.ToLower() == normalized.ToLower() && f.Status == "Active");
    }

    public async Task AddAsync(Fare fare)
    {
        await _context.Fares.AddAsync(fare);
    }

    public Task UpdateAsync(Fare fare)
    {
        _context.Fares.Update(fare);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
