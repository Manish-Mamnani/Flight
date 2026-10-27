using System.ComponentModel.DataAnnotations;

namespace SearchService.DTOs;

public class UpdateFlightDto
{
    [StringLength(10, MinimumLength = 2)]
    public string? FlightNumber { get; set; }

    public int? AirlineId { get; set; }

    public int? OriginAirportId { get; set; }

    public int? DestinationAirportId { get; set; }

    public string? Status { get; set; }
}
