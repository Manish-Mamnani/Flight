using System.ComponentModel.DataAnnotations;

namespace FareService.DTOs;

public class UpdateFareDto
{
    [Range(0, 1000000)]
    public decimal? BaseFare { get; set; }

    [Range(0, 1000000)]
    public decimal? TaxAmount { get; set; }

    [RegularExpression("^[A-Z]{3}$")]
    public string? CurrencyCode { get; set; }

    public string? Status { get; set; }
}
