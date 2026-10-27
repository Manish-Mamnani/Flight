using System.ComponentModel.DataAnnotations;

namespace CheckInService.DTOs;

public class CheckInRequestDto
{
    public int? BookingId { get; set; }
    public string? BookingReference { get; set; }

    [Required]
    public int PassengerId { get; set; }

    [Required]
    public string SeatNumber { get; set; } = string.Empty;

    public string? Segment { get; set; }
}
