using CheckInService.DTOs;

namespace CheckInService.HttpClients;

public interface IBookingServiceClient
{
    Task<BookingDetailsResponseDto?> GetBookingValidationAsync(string referenceOrId);
}
