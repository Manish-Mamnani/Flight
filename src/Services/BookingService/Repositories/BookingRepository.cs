using BookingService.Data;
using BookingService.Models;
using BookingService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _context;

    public BookingRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(int bookingId)
    {
        return await _context.Bookings
            .Include(b => b.Passengers)
            .Include(b => b.StatusHistory)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);
    }

    public async Task<Booking?> GetByReferenceAsync(string bookingReference)
    {
        var normalized = bookingReference.Trim().ToUpperInvariant();
        return await _context.Bookings
            .Include(b => b.Passengers)
            .Include(b => b.StatusHistory)
            .FirstOrDefaultAsync(b => b.BookingReference == normalized);
    }

    public async Task<IEnumerable<Booking>> GetByUserIdAsync(int userId)
    {
        return await _context.Bookings
            .Include(b => b.Passengers)
            .Include(b => b.StatusHistory)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookedAtUtc)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetByUserIdAsync(string userId)
    {
        if (int.TryParse(userId, out var parsedId))
        {
            return await GetByUserIdAsync(parsedId);
        }

        return Enumerable.Empty<Booking>();
    }

    public async Task AddAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
    }

    public Task UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
