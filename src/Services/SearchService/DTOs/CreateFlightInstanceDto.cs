using System.ComponentModel.DataAnnotations;

namespace SearchService.DTOs;

public class CreateFlightInstanceDto
{
    [Required]
    public DateTime DepartureAtUtc { get; set; }

    [Required]
    public DateTime ArrivalAtUtc { get; set; }

    [Range(1, 1000)]
    public int TotalSeats { get; set; }
}
