namespace BookingService.Models;

public class Passenger
{
    public int PassengerId { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string PassengerType { get; set; } = "Adult";
    public string? PassportNumber { get; set; }
}
