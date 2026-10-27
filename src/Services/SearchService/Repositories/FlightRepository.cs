using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Models;
using SearchService.Repositories.Interfaces;

namespace SearchService.Repositories;

public class FlightRepository : IFlightRepository
{
    private readonly SearchDbContext _context;

    public FlightRepository(SearchDbContext context)
    {
        _context = context;
    }

    public async Task<Flight?> GetByIdAsync(int flightId)
    {
        return await _context.Flights.FindAsync(flightId);
    }

    public async Task<Flight?> GetByIdWithDetailsAsync(int flightId)
    {
        return await _context.Flights
            .Include(f => f.Airline)
            .Include(f => f.OriginAirport)
            .Include(f => f.DestinationAirport)
            .Include(f => f.FlightInstances)
            .FirstOrDefaultAsync(f => f.FlightId == flightId);
    }

    public async Task<IEnumerable<Flight>> SearchFlightsAsync(string originCode, string destinationCode, DateTime departureDate)
    {
        var origin = originCode.Trim().ToUpperInvariant();
        var destination = destinationCode.Trim().ToUpperInvariant();
        var searchDate = departureDate.Date;

        return await _context.Flights
            .Include(f => f.Airline)
            .Include(f => f.OriginAirport)
            .Include(f => f.DestinationAirport)
            .Include(f => f.FlightInstances.Where(fi => fi.DepartureAtUtc.Date == searchDate && fi.FlightStatus != "Cancelled"))
            .Where(f => f.Status == "Active"
                     && f.OriginAirport.AirportCode == origin
                     && f.DestinationAirport.AirportCode == destination
                     && f.FlightInstances.Any(fi => fi.DepartureAtUtc.Date == searchDate && fi.FlightStatus != "Cancelled"))
            .ToListAsync();
    }

    public async Task<IEnumerable<Flight>> GetByManagerIdAsync(int managerId)
    {
        return await _context.Flights
            .Include(f => f.Airline)
            .Include(f => f.OriginAirport)
            .Include(f => f.DestinationAirport)
            .Include(f => f.FlightInstances)
            .Where(f => f.CreatedByUserId == managerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Flight>> GetAllManagedAsync()
    {
        return await _context.Flights
            .Include(f => f.Airline)
            .Include(f => f.OriginAirport)
            .Include(f => f.DestinationAirport)
            .Include(f => f.FlightInstances)
            .ToListAsync();
    }

    public async Task AddAsync(Flight flight)
    {
        await _context.Flights.AddAsync(flight);
    }

    public Task UpdateAsync(Flight flight)
    {
        _context.Flights.Update(flight);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
