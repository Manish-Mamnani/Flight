namespace SearchService.DTOs;

public class FlightValidationDto
{
    public int FlightInstanceId { get; set; }
    public int FlightId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string OriginCode { get; set; } = string.Empty;
    public string DestinationCode { get; set; } = string.Empty;
    public DateTime DepartureAtUtc { get; set; }
    public DateTime ArrivalAtUtc { get; set; }
    public int TotalSeats { get; set; }
    public string FlightStatus { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public bool IsActive { get; set; }
}
