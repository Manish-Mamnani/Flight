using System.ComponentModel.DataAnnotations;

namespace SearchService.DTOs;

public class CreateFlightDto
{
    [Required]
    [StringLength(10, MinimumLength = 2)]
    public string FlightNumber { get; set; } = string.Empty;

    [Required]
    public int AirlineId { get; set; }

    [Required]
    public int OriginAirportId { get; set; }

    [Required]
    public int DestinationAirportId { get; set; }

    public List<CreateFlightInstanceDto> Instances { get; set; } = new();
}
