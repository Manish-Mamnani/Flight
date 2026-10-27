using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs;

public class RegisterDto
{
    [StringLength(50, MinimumLength = 1)]
    public string? FirstName { get; set; }

    [StringLength(50, MinimumLength = 1)]
    public string? LastName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(250)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [StringLength(15)]
    public string? PhoneNumber { get; set; }
}
