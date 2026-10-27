namespace BookingService.DTOs;

public class FlightValidationResultDto
{
    public int FlightInstanceId { get; set; }
    public int FlightId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DepartureAtUtc { get; set; }
    public DateTime ArrivalAtUtc { get; set; }
    public string FlightStatus { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
