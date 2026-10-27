namespace CheckInService.Models;

public class FlightSeatInventory
{
    public int SeatId { get; set; }
    public int FlightInstanceId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public string SeatClass { get; set; } = "Economy";
    public string SeatStatus { get; set; } = "Available";
    public int? BookingId { get; set; }
    public int? PassengerId { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
