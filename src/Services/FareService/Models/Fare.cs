namespace FareService.Models;

public class Fare
{
    public int FareId { get; set; }
    public int FlightInstanceId { get; set; }
    public string FareClass { get; set; } = "Economy";
    public decimal BaseFare { get; set; }
    public decimal TaxAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string Status { get; set; } = "Active";

    public decimal TotalFare => BaseFare + TaxAmount;
}
