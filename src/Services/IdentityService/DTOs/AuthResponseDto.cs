namespace IdentityService.DTOs;

public class AuthResponseDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public string Token { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
