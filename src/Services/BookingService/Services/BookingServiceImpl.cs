using BookingService.DTOs;
using BookingService.Exceptions;
using BookingService.HttpClients;
using BookingService.Models;
using BookingService.Repositories.Interfaces;
using BookingService.Services.Interfaces;

namespace BookingService.Services;

public class BookingServiceImpl : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ISearchServiceClient _searchServiceClient;
    private readonly IFareServiceClient _fareServiceClient;
    private readonly ICheckInServiceClient _checkInServiceClient;

    public BookingServiceImpl(
        IBookingRepository bookingRepository,
        ISearchServiceClient searchServiceClient,
        IFareServiceClient fareServiceClient,
        ICheckInServiceClient checkInServiceClient)
    {
        _bookingRepository = bookingRepository;
        _searchServiceClient = searchServiceClient;
        _fareServiceClient = fareServiceClient;
        _checkInServiceClient = checkInServiceClient;
    }

    public async Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto dto, int userId)
    {
        if (dto.Passengers == null || dto.Passengers.Count == 0)
        {
            throw new ArgumentException("at least one passenger required");
        }

        if (dto.Passengers.Count > 9)
        {
            throw new InvalidBookingOperationException("Maximum 9 passengers allowed per booking.");
        }

        var instanceId = dto.FlightInstanceId ?? dto.FlightId ?? 0;
        if (instanceId == 0)
        {
            throw new ArgumentException("Flight or flight instance ID is required.");
        }

        // Validate flight instance with SearchService
        var flightInstance = await _searchServiceClient.ValidateFlightInstanceAsync(instanceId);
        if (flightInstance == null)
        {
            // If downstream mock returns null or flight not found
            throw new BookingNotFoundException("Flight not found");
        }

        decimal baseFarePerPassenger = 5000m;
        decimal taxPerPassenger = 0m;
        string currencyCode = "USD";

        // Validate fare with FareService
        var fareResult = await _fareServiceClient.ValidateFareAsync(dto.FareId, instanceId);
        if (fareResult != null)
        {
            baseFarePerPassenger = fareResult.BaseFare;
            taxPerPassenger = fareResult.TaxAmount;
            currencyCode = fareResult.CurrencyCode;
        }

        var passengerCount = dto.Passengers.Count;
        var baseFareTotal = baseFarePerPassenger * passengerCount;
        var taxTotal = taxPerPassenger * passengerCount;
        var grandTotal = baseFareTotal + taxTotal;

        var pnr = GenerateBookingReference();

        var booking = new Booking
        {
            BookingReference = pnr,
            UserId = userId,
            FlightInstanceId = instanceId,
            FareId = dto.FareId,
            BookingStatus = "Confirmed",
            BaseFareTotal = baseFareTotal,
            TaxTotal = taxTotal,
            GrandTotal = grandTotal,
            CurrencyCode = currencyCode,
            BookedAtUtc = DateTime.UtcNow
        };

        foreach (var p in dto.Passengers)
        {
            booking.Passengers.Add(new Passenger
            {
                BookingId = booking.BookingId,
                FirstName = p.FirstName.Trim(),
                LastName = p.LastName.Trim(),
                DateOfBirth = p.DateOfBirth,
                Gender = p.Gender?.Trim(),
                PassengerType = string.IsNullOrWhiteSpace(p.PassengerType) ? "Adult" : p.PassengerType.Trim(),
                PassportNumber = p.PassportNumber?.Trim()
            });
        }

        booking.StatusHistory.Add(new BookingStatusHistory
        {
            BookingId = booking.BookingId,
            PreviousStatus = null,
            NewStatus = "Confirmed",
            ChangedAtUtc = DateTime.UtcNow,
            ChangedByUserId = userId,
            Remarks = "Booking created and confirmed."
        });

        await _bookingRepository.AddAsync(booking);
        await _bookingRepository.SaveChangesAsync();

        return MapToBookingResponseDto(booking);
    }

    public async Task<IEnumerable<BookingResponseDto>> GetMyBookingsAsync(int userId)
    {
        var bookings = await _bookingRepository.GetByUserIdAsync(userId);
        return bookings.Select(MapToBookingResponseDto);
    }

    public async Task<BookingResponseDto> GetBookingAsync(string referenceOrId, int userId, bool isAdmin)
    {
        var booking = await FindBookingAsync(referenceOrId);
        if (booking == null)
        {
            throw new BookingNotFoundException("Booking not found");
        }

        if (!isAdmin && booking.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this booking.");
        }

        return MapToBookingResponseDto(booking);
    }

    public async Task<BookingResponseDto> CancelBookingAsync(string referenceOrId, int userId, bool isAdmin)
    {
        var booking = await FindBookingAsync(referenceOrId);
        if (booking == null)
        {
            throw new BookingNotFoundException("Booking not found");
        }

        if (!isAdmin && booking.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to cancel this booking.");
        }

        if (booking.BookingStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidBookingOperationException("Booking is already cancelled.");
        }

        var prevStatus = booking.BookingStatus;
        booking.BookingStatus = "Cancelled";
        booking.CancelledAtUtc = DateTime.UtcNow;

        booking.StatusHistory.Add(new BookingStatusHistory
        {
            BookingId = booking.BookingId,
            PreviousStatus = prevStatus,
            NewStatus = "Cancelled",
            ChangedAtUtc = DateTime.UtcNow,
            ChangedByUserId = userId,
            Remarks = "Booking cancelled by user."
        });

        await _bookingRepository.UpdateAsync(booking);
        await _bookingRepository.SaveChangesAsync();

        // Release seats asynchronously via CheckInService
        await _checkInServiceClient.ReleaseSeatsAsync(booking.BookingId);

        return MapToBookingResponseDto(booking);
    }

    public async Task<IEnumerable<PassengerDto>> GetPassengersAsync(string referenceOrId, int userId, bool isAdmin)
    {
        var booking = await FindBookingAsync(referenceOrId);
        if (booking == null)
        {
            throw new BookingNotFoundException("Booking not found");
        }

        if (!isAdmin && booking.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this booking's passengers.");
        }

        return booking.Passengers.Select(p => new PassengerDto
        {
            PassengerId = p.PassengerId,
            BookingId = p.BookingId,
            FirstName = p.FirstName,
            LastName = p.LastName,
            DateOfBirth = p.DateOfBirth,
            Gender = p.Gender,
            PassengerType = p.PassengerType,
            PassportNumber = MaskPassport(p.PassportNumber)
        });
    }

    public async Task<BookingValidationDto> ValidateBookingForCheckInAsync(string referenceOrId)
    {
        var booking = await FindBookingAsync(referenceOrId);
        if (booking == null)
        {
            throw new BookingNotFoundException("Booking not found");
        }

        return new BookingValidationDto
        {
            BookingId = booking.BookingId,
            BookingReference = booking.BookingReference,
            UserId = booking.UserId,
            FlightInstanceId = booking.FlightInstanceId,
            BookingStatus = booking.BookingStatus,
            PassengerIds = booking.Passengers.Select(p => p.PassengerId).ToList()
        };
    }

    private async Task<Booking?> FindBookingAsync(string referenceOrId)
    {
        if (int.TryParse(referenceOrId, out var id))
        {
            var byId = await _bookingRepository.GetByIdAsync(id);
            if (byId != null) return byId;
        }

        return await _bookingRepository.GetByReferenceAsync(referenceOrId);
    }

    private static string GenerateBookingReference()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        var buffer = new char[6];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = chars[random.Next(chars.Length)];
        }
        return $"SK{new string(buffer)}";
    }

    private static string? MaskPassport(string? passport)
    {
        if (string.IsNullOrWhiteSpace(passport)) return null;
        if (passport.Length <= 4) return new string('*', passport.Length);
        return new string('*', passport.Length - 4) + passport[^4..];
    }

    private static BookingResponseDto MapToBookingResponseDto(Booking booking)
    {
        return new BookingResponseDto
        {
            BookingId = booking.BookingId,
            BookingReference = booking.BookingReference,
            UserId = booking.UserId,
            FlightInstanceId = booking.FlightInstanceId,
            FareId = booking.FareId,
            BookingStatus = booking.BookingStatus,
            BaseFareTotal = booking.BaseFareTotal,
            TaxTotal = booking.TaxTotal,
            GrandTotal = booking.GrandTotal,
            CurrencyCode = booking.CurrencyCode,
            BookedAtUtc = booking.BookedAtUtc,
            CancelledAtUtc = booking.CancelledAtUtc,
            Passengers = booking.Passengers.Select(p => new PassengerDto
            {
                PassengerId = p.PassengerId,
                BookingId = p.BookingId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                DateOfBirth = p.DateOfBirth,
                Gender = p.Gender,
                PassengerType = p.PassengerType,
                PassportNumber = MaskPassport(p.PassportNumber)
            }).ToList(),
            StatusHistory = booking.StatusHistory.Select(h => new BookingStatusHistoryDto
            {
                HistoryId = h.HistoryId,
                BookingId = h.BookingId,
                PreviousStatus = h.PreviousStatus,
                NewStatus = h.NewStatus,
                ChangedAtUtc = h.ChangedAtUtc,
                ChangedByUserId = h.ChangedByUserId,
                Remarks = h.Remarks
            }).ToList()
        };
    }
}
