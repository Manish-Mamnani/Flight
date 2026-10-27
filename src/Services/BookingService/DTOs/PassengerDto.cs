namespace BookingService.DTOs;

public class PassengerDto
{
    public int PassengerId { get; set; }
    public int BookingId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string PassengerType { get; set; } = string.Empty;
    public string? PassportNumber { get; set; }
}
