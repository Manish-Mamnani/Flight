namespace BookingService.DTOs;

public class BookingStatusHistoryDto
{
    public long HistoryId { get; set; }
    public int BookingId { get; set; }
    public string? PreviousStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAtUtc { get; set; }
    public int? ChangedByUserId { get; set; }
    public string? Remarks { get; set; }
}
