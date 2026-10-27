using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Models;
using SearchService.Repositories.Interfaces;

namespace SearchService.Repositories;

public class AirlineRepository : IAirlineRepository
{
    private readonly SearchDbContext _context;

    public AirlineRepository(SearchDbContext context)
    {
        _context = context;
    }

    public async Task<Airline?> GetByIdAsync(int airlineId)
    {
        return await _context.Airlines.FindAsync(airlineId);
    }

    public async Task<Airline?> GetByCodeAsync(string airlineCode)
    {
        var normalized = airlineCode.Trim().ToUpperInvariant();
        return await _context.Airlines.FirstOrDefaultAsync(a => a.AirlineCode == normalized);
    }

    public async Task<IEnumerable<Airline>> GetAllActiveAsync()
    {
        return await _context.Airlines.Where(a => a.Status == "Active").ToListAsync();
    }

    public async Task AddAsync(Airline airline)
    {
        await _context.Airlines.AddAsync(airline);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
