using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs;

public class UpdateProfileDto
{
    [StringLength(50, MinimumLength = 1)]
    public string? FirstName { get; set; }

    [StringLength(50, MinimumLength = 1)]
    public string? LastName { get; set; }

    [StringLength(15)]
    public string? PhoneNumber { get; set; }
}
