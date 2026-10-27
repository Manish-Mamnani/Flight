using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Models;
using SearchService.Repositories.Interfaces;

namespace SearchService.Repositories;

public class AirportRepository : IAirportRepository
{
    private readonly SearchDbContext _context;

    public AirportRepository(SearchDbContext context)
    {
        _context = context;
    }

    public async Task<Airport?> GetByIdAsync(int airportId)
    {
        return await _context.Airports.FindAsync(airportId);
    }

    public async Task<Airport?> GetByCodeAsync(string airportCode)
    {
        var normalized = airportCode.Trim().ToUpperInvariant();
        return await _context.Airports.FirstOrDefaultAsync(a => a.AirportCode == normalized);
    }

    public async Task<IEnumerable<Airport>> GetAllActiveAsync()
    {
        return await _context.Airports.Where(a => a.Status == "Active").ToListAsync();
    }

    public async Task AddAsync(Airport airport)
    {
        await _context.Airports.AddAsync(airport);
    }

    public Task UpdateAsync(Airport airport)
    {
        _context.Airports.Update(airport);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
