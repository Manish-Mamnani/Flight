namespace SearchService.DTOs;

public class AirportDto
{
    public int AirportId { get; set; }
    public string AirportCode { get; set; } = string.Empty;
    public string AirportName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
