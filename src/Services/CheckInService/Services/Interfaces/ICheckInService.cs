using CheckInService.DTOs;

namespace CheckInService.Services.Interfaces;

public interface ICheckInService
{
    Task<IEnumerable<SeatDto>> GetAvailableSeatsAsync(string referenceOrId);
    Task<CheckInResponseDto> CheckInAsync(string referenceOrId, CheckInRequestDto request, int currentUserId, bool isAdmin);
    Task<CheckInResponseDto> CheckInAsync(CheckInRequestDto request);
    Task<IEnumerable<SeatDto>> GetCheckInStatusAsync(string referenceOrId);
    Task<bool> ReleaseSeatsForBookingAsync(int bookingId);
}
