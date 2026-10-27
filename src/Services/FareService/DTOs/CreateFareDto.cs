using System.ComponentModel.DataAnnotations;

namespace FareService.DTOs;

public class CreateFareDto
{
    [Required]
    public int FlightInstanceId { get; set; }

    [Required]
    [StringLength(20)]
    public string FareClass { get; set; } = "Economy";

    [Range(0, 1000000)]
    public decimal BaseFare { get; set; }

    [Range(0, 1000000)]
    public decimal TaxAmount { get; set; }

    [Required]
    [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Currency code must be a 3-letter code.")]
    public string CurrencyCode { get; set; } = "USD";
}
