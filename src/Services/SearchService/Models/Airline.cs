namespace SearchService.Models;

public class Airline
{
    public int AirlineId { get; set; }
    public string AirlineCode { get; set; } = string.Empty;
    public string AirlineName { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";

    public ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
