using CheckInService.Models;

namespace CheckInService.Repositories.Interfaces;

public interface ICheckInRepository
{
    Task<FlightSeatInventory?> GetSeatAsync(int flightInstanceId, string seatNumber);
    Task<FlightSeatInventory?> GetAssignedSeatForPassengerAsync(int flightInstanceId, int passengerId);
    Task<IEnumerable<FlightSeatInventory>> GetAvailableSeatsAsync(int flightInstanceId);
    Task<IEnumerable<FlightSeatInventory>> GetSeatsByBookingIdAsync(int bookingId);
    Task AddAsync(FlightSeatInventory seat);
    Task UpdateAsync(FlightSeatInventory seat);
    Task SaveChangesAsync();
}
