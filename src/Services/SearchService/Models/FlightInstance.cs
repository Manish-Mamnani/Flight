namespace SearchService.Models;

public class FlightInstance
{
    public int FlightInstanceId { get; set; }
    public int FlightId { get; set; }
    public Flight Flight { get; set; } = null!;
    public DateTime DepartureAtUtc { get; set; }
    public DateTime ArrivalAtUtc { get; set; }
    public int TotalSeats { get; set; }
    public string FlightStatus { get; set; } = "Scheduled";
}
