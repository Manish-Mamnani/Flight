namespace FareService.DTOs;

public class FlightValidationResponseDto
{
    public int FlightInstanceId { get; set; }
    public int FlightId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public bool IsActive { get; set; }
}
