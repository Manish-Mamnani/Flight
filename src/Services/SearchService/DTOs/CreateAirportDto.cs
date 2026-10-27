using System.ComponentModel.DataAnnotations;

namespace SearchService.DTOs;

public class CreateAirportDto
{
    [Required]
    [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Airport code must be exactly 3 uppercase letters.")]
    public string AirportCode { get; set; } = string.Empty;

    [Required]
    [StringLength(150, MinimumLength = 1)]
    public string AirportName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Country { get; set; } = string.Empty;
}
