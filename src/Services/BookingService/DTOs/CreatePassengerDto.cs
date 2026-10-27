using System.ComponentModel.DataAnnotations;

namespace BookingService.DTOs;

public class CreatePassengerDto
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [StringLength(20)]
    public string? Gender { get; set; }

    [Required]
    [StringLength(20)]
    public string PassengerType { get; set; } = "Adult";

    [StringLength(30)]
    public string? PassportNumber { get; set; }
}
