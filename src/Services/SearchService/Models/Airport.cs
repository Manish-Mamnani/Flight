namespace SearchService.Models;

public class Airport
{
    public int AirportId { get; set; }
    public string AirportCode { get; set; } = string.Empty;
    public string AirportName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";

    public ICollection<Flight> DepartingFlights { get; set; } = new List<Flight>();
    public ICollection<Flight> ArrivingFlights { get; set; } = new List<Flight>();
}
