namespace BookingService.DTOs;

public class BookingValidationDto
{
    public int BookingId { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int FlightInstanceId { get; set; }
    public string BookingStatus { get; set; } = string.Empty;
    public List<int> PassengerIds { get; set; } = new();
}
