using System.ComponentModel.DataAnnotations;

namespace SearchService.DTOs;

public class UpdateAirportDto
{
    [StringLength(150, MinimumLength = 1)]
    public string? AirportName { get; set; }

    [StringLength(100, MinimumLength = 1)]
    public string? City { get; set; }

    [StringLength(100, MinimumLength = 1)]
    public string? Country { get; set; }

    [StringLength(30)]
    public string? Status { get; set; }
}
