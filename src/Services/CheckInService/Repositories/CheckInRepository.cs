using CheckInService.Data;
using CheckInService.Models;
using CheckInService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CheckInService.Repositories;

public class CheckInRepository : ICheckInRepository
{
    private readonly CheckInDbContext _context;

    public CheckInRepository(CheckInDbContext context)
    {
        _context = context;
    }

    public async Task<FlightSeatInventory?> GetSeatAsync(int flightInstanceId, string seatNumber)
    {
        var normalized = seatNumber.Trim().ToUpperInvariant();
        return await _context.SeatInventories
            .FirstOrDefaultAsync(s => s.FlightInstanceId == flightInstanceId && s.SeatNumber == normalized);
    }

    public async Task<FlightSeatInventory?> GetAssignedSeatForPassengerAsync(int flightInstanceId, int passengerId)
    {
        return await _context.SeatInventories
            .FirstOrDefaultAsync(s => s.FlightInstanceId == flightInstanceId && s.PassengerId == passengerId && s.SeatStatus == "Assigned");
    }

    public async Task<IEnumerable<FlightSeatInventory>> GetAvailableSeatsAsync(int flightInstanceId)
    {
        return await _context.SeatInventories
            .Where(s => s.FlightInstanceId == flightInstanceId && s.SeatStatus == "Available")
            .ToListAsync();
    }

    public async Task<IEnumerable<FlightSeatInventory>> GetSeatsByBookingIdAsync(int bookingId)
    {
        return await _context.SeatInventories
            .Where(s => s.BookingId == bookingId)
            .ToListAsync();
    }

    public async Task AddAsync(FlightSeatInventory seat)
    {
        await _context.SeatInventories.AddAsync(seat);
    }

    public Task UpdateAsync(FlightSeatInventory seat)
    {
        _context.SeatInventories.Update(seat);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
