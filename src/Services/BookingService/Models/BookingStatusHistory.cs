namespace BookingService.Models;

public class BookingStatusHistory
{
    public long HistoryId { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public string? PreviousStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
    public int? ChangedByUserId { get; set; }
    public string? Remarks { get; set; }
}
