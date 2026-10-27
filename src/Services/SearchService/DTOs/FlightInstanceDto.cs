namespace SearchService.DTOs;

public class FlightInstanceDto
{
    public int FlightInstanceId { get; set; }
    public int FlightId { get; set; }
    public DateTime DepartureAtUtc { get; set; }
    public DateTime ArrivalAtUtc { get; set; }
    public int TotalSeats { get; set; }
    public string FlightStatus { get; set; } = string.Empty;
}
