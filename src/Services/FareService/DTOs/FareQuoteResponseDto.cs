namespace FareService.DTOs;

public class FareQuoteResponseDto
{
    public int FareId { get; set; }
    public int FlightId { get; set; }
    public int FlightInstanceId { get; set; }
    public string FareClass { get; set; } = string.Empty;
    public decimal PerPassengerFare { get; set; }
    public int PassengerCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
