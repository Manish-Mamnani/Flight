using BookingService.DTOs;

namespace BookingService.Services.Interfaces;

public interface IBookingService
{
    Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto dto, int userId);
    Task<IEnumerable<BookingResponseDto>> GetMyBookingsAsync(int userId);
    Task<BookingResponseDto> GetBookingAsync(string referenceOrId, int userId, bool isAdmin);
    Task<BookingResponseDto> CancelBookingAsync(string referenceOrId, int userId, bool isAdmin);
    Task<IEnumerable<PassengerDto>> GetPassengersAsync(string referenceOrId, int userId, bool isAdmin);
    Task<BookingValidationDto> ValidateBookingForCheckInAsync(string referenceOrId);
}
