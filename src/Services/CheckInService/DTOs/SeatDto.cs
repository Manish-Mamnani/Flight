namespace CheckInService.DTOs;

public class SeatDto
{
    public int SeatId { get; set; }
    public int FlightInstanceId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public string SeatClass { get; set; } = string.Empty;
    public string SeatStatus { get; set; } = string.Empty;
    public int? BookingId { get; set; }
    public int? PassengerId { get; set; }
}
