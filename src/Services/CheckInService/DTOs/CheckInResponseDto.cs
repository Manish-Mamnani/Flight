namespace CheckInService.DTOs;

public class CheckInResponseDto
{
    public int SeatId { get; set; }
    public int FlightInstanceId { get; set; }
    public int BookingId { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public int PassengerId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public string SeatClass { get; set; } = string.Empty;
    public string Status { get; set; } = "CHECKED_IN";
    public DateTime CheckedInAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}
