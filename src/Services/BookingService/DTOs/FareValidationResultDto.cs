namespace BookingService.DTOs;

public class FareValidationResultDto
{
    public int FareId { get; set; }
    public int FlightInstanceId { get; set; }
    public string FareClass { get; set; } = string.Empty;
    public decimal BaseFare { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalFare => BaseFare + TaxAmount;
    public string CurrencyCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
