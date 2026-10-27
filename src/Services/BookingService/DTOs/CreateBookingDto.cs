using System.ComponentModel.DataAnnotations;

namespace BookingService.DTOs;

public class CreateBookingDto
{
    public int? FlightId { get; set; }

    public int? FlightInstanceId { get; set; }

    [Required]
    public int FareId { get; set; }

    public List<CreatePassengerDto> Passengers { get; set; } = new();
}
