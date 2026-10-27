namespace SearchService.DTOs;

public class FlightResponseDto
{
    public int FlightId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string AirlineCode { get; set; } = string.Empty;
    public string AirlineName { get; set; } = string.Empty;
    public string OriginAirportCode { get; set; } = string.Empty;
    public string OriginAirportName { get; set; } = string.Empty;
    public string OriginCity { get; set; } = string.Empty;
    public string DestinationAirportCode { get; set; } = string.Empty;
    public string DestinationAirportName { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<FlightInstanceDto> Instances { get; set; } = new();
}
