using CheckInService.DTOs;
using CheckInService.Exceptions;
using CheckInService.HttpClients;
using CheckInService.Models;
using CheckInService.Repositories.Interfaces;
using CheckInService.Services.Interfaces;

namespace CheckInService.Services;

public class CheckInServiceImpl : ICheckInService
{
    private readonly ICheckInRepository _checkInRepository;
    private readonly IBookingServiceClient _bookingServiceClient;
    private readonly ISearchServiceClient _searchServiceClient;

    public CheckInServiceImpl(
        ICheckInRepository checkInRepository,
        IBookingServiceClient bookingServiceClient,
        ISearchServiceClient searchServiceClient)
    {
        _checkInRepository = checkInRepository;
        _bookingServiceClient = bookingServiceClient;
        _searchServiceClient = searchServiceClient;
    }

    public async Task<IEnumerable<SeatDto>> GetAvailableSeatsAsync(string referenceOrId)
    {
        var booking = await _bookingServiceClient.GetBookingValidationAsync(referenceOrId);
        var instanceId = booking?.FlightInstanceId ?? (int.TryParse(referenceOrId, out var parsedId) ? parsedId : 0);

        var availableSeats = await _checkInRepository.GetAvailableSeatsAsync(instanceId);
        return availableSeats.Select(MapToSeatDto);
    }

    public async Task<CheckInResponseDto> CheckInAsync(CheckInRequestDto request)
    {
        var reference = !string.IsNullOrWhiteSpace(request.BookingReference)
            ? request.BookingReference
            : request.BookingId?.ToString() ?? "0";

        return await CheckInAsync(reference, request, 0, isAdmin: true);
    }

    public async Task<CheckInResponseDto> CheckInAsync(
        string referenceOrId,
        CheckInRequestDto request,
        int currentUserId,
        bool isAdmin)
    {
        if (string.IsNullOrWhiteSpace(request.SeatNumber))
        {
            throw new ArgumentException("Seat number is required");
        }

        var booking = await _bookingServiceClient.GetBookingValidationAsync(referenceOrId);

        // Validation against Booking details
        if (booking != null)
        {
            if (!isAdmin && currentUserId != 0 && booking.UserId != currentUserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to check in for this booking.");
            }

            if (!booking.BookingStatus.Equals("Confirmed", StringComparison.OrdinalIgnoreCase))
            {
                throw new CheckInException("booking is not active");
            }

            if (booking.PassengerIds != null && booking.PassengerIds.Count > 0 && !booking.PassengerIds.Contains(request.PassengerId))
            {
                throw new KeyNotFoundException("passenger not found on booking");
            }
        }

        var instanceId = booking?.FlightInstanceId ?? (int.TryParse(referenceOrId, out var parsedId) ? parsedId : 0);

        // Validate flight instance window with SearchService
        if (booking != null)
        {
            var flight = await _searchServiceClient.GetFlightInstanceAsync(instanceId);
            if (flight != null)
            {
                var now = DateTime.UtcNow;
                var departure = flight.DepartureAtUtc;

                // Check-in window: 48 hours prior to departure up until departure time
                if (now < departure.AddHours(-48) || now > departure)
                {
                    throw new CheckInWindowClosedException("check-in not open");
                }
            }
        }

        // Check if passenger is already checked in (repeat check-in)
        var existingPassengerSeat = await _checkInRepository.GetAssignedSeatForPassengerAsync(instanceId, request.PassengerId);
        if (existingPassengerSeat != null)
        {
            return new CheckInResponseDto
            {
                SeatId = existingPassengerSeat.SeatId,
                FlightInstanceId = existingPassengerSeat.FlightInstanceId,
                BookingId = existingPassengerSeat.BookingId ?? 0,
                BookingReference = referenceOrId,
                PassengerId = request.PassengerId,
                SeatNumber = existingPassengerSeat.SeatNumber,
                SeatClass = existingPassengerSeat.SeatClass,
                Status = "CHECKED_IN",
                CheckedInAtUtc = existingPassengerSeat.UpdatedAtUtc,
                Message = "Passenger is already checked in."
            };
        }

        // Check seat availability
        var seat = await _checkInRepository.GetSeatAsync(instanceId, request.SeatNumber);

        if (seat != null)
        {
            if (seat.SeatStatus.Equals("Assigned", StringComparison.OrdinalIgnoreCase))
            {
                throw new SeatAlreadyAssignedException("seat already assigned");
            }

            seat.SeatStatus = "Assigned";
            seat.BookingId = booking?.BookingId ?? request.BookingId;
            seat.PassengerId = request.PassengerId;
            seat.UpdatedAtUtc = DateTime.UtcNow;

            await _checkInRepository.UpdateAsync(seat);
        }
        else
        {
            // First time seat allocation for this instance
            seat = new FlightSeatInventory
            {
                // SeatId auto-generated by EF
                FlightInstanceId = instanceId,
                SeatNumber = request.SeatNumber.Trim().ToUpperInvariant(),
                SeatClass = "Economy",
                SeatStatus = "Assigned",
                BookingId = booking?.BookingId ?? request.BookingId,
                PassengerId = request.PassengerId,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await _checkInRepository.AddAsync(seat);
        }

        await _checkInRepository.SaveChangesAsync();

        return new CheckInResponseDto
        {
            SeatId = seat.SeatId,
            FlightInstanceId = seat.FlightInstanceId,
            BookingId = seat.BookingId ?? 0,
            BookingReference = referenceOrId,
            PassengerId = request.PassengerId,
            SeatNumber = seat.SeatNumber,
            SeatClass = seat.SeatClass,
            Status = "CHECKED_IN",
            CheckedInAtUtc = seat.UpdatedAtUtc,
            Message = "Check-in successful."
        };
    }

    public async Task<IEnumerable<SeatDto>> GetCheckInStatusAsync(string referenceOrId)
    {
        var booking = await _bookingServiceClient.GetBookingValidationAsync(referenceOrId);
        var bookingId = booking?.BookingId ?? (int.TryParse(referenceOrId, out var parsedBookingId) ? parsedBookingId : 0);

        var seats = await _checkInRepository.GetSeatsByBookingIdAsync(bookingId);
        return seats.Select(MapToSeatDto);
    }

    public async Task<bool> ReleaseSeatsForBookingAsync(int bookingId)
    {
        var seats = await _checkInRepository.GetSeatsByBookingIdAsync(bookingId);
        foreach (var seat in seats)
        {
            seat.SeatStatus = "Available";
            seat.BookingId = null;
            seat.PassengerId = null;
            seat.UpdatedAtUtc = DateTime.UtcNow;
            await _checkInRepository.UpdateAsync(seat);
        }

        await _checkInRepository.SaveChangesAsync();
        return true;
    }

    private static SeatDto MapToSeatDto(FlightSeatInventory seat)
    {
        return new SeatDto
        {
            SeatId = seat.SeatId,
            FlightInstanceId = seat.FlightInstanceId,
            SeatNumber = seat.SeatNumber,
            SeatClass = seat.SeatClass,
            SeatStatus = seat.SeatStatus,
            BookingId = seat.BookingId,
            PassengerId = seat.PassengerId
        };
    }
}
