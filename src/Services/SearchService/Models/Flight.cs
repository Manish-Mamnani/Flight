namespace SearchService.Models;

public class Flight
{
    public int FlightId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public int AirlineId { get; set; }
    public Airline Airline { get; set; } = null!;
    public int OriginAirportId { get; set; }
    public Airport OriginAirport { get; set; } = null!;
    public int DestinationAirportId { get; set; }
    public Airport DestinationAirport { get; set; } = null!;
    public int CreatedByUserId { get; set; }
    public string Status { get; set; } = "Active";

    public ICollection<FlightInstance> FlightInstances { get; set; } = new List<FlightInstance>();
}
