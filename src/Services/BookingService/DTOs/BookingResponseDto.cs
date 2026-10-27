namespace BookingService.DTOs;

public class BookingResponseDto
{
    public int BookingId { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int FlightInstanceId { get; set; }
    public int FareId { get; set; }
    public string BookingStatus { get; set; } = string.Empty;
    public decimal BaseFareTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal TotalAmount => GrandTotal;
    public string CurrencyCode { get; set; } = string.Empty;
    public DateTime BookedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public List<PassengerDto> Passengers { get; set; } = new();
    public List<BookingStatusHistoryDto> StatusHistory { get; set; } = new();
}
