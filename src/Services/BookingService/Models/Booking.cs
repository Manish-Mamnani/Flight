namespace BookingService.Models;

public class Booking
{
    public int BookingId { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int FlightInstanceId { get; set; }
    public int FareId { get; set; }
    public string BookingStatus { get; set; } = "Confirmed";
    public decimal BaseFareTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime BookedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAtUtc { get; set; }
    public byte[]? RowVersion { get; set; }

    public ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();
    public ICollection<BookingStatusHistory> StatusHistory { get; set; } = new List<BookingStatusHistory>();
}
