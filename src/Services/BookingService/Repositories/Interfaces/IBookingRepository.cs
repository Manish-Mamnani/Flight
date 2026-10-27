using BookingService.Models;

namespace BookingService.Repositories.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(int bookingId);
    Task<Booking?> GetByReferenceAsync(string bookingReference);
    Task<IEnumerable<Booking>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Booking>> GetByUserIdAsync(string userId);
    Task AddAsync(Booking booking);
    Task UpdateAsync(Booking booking);
    Task SaveChangesAsync();
}
