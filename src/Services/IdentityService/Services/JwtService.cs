using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IdentityService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Services;

public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user, IEnumerable<string> roles)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? _configuration["Jwt:Secret"] ?? "SkyLinkSuperSecretKeyThatIsAtLeast32BytesLong!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new("UserId", user.UserId.ToString()),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new(JwtRegisteredClaimNames.Jti, RandomNumberGenerator.GetInt32(1, int.MaxValue).ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim("Role", role));
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var issuer = _configuration["Jwt:Issuer"] ?? "SkyLink";
        var audience = _configuration["Jwt:Audience"] ?? "SkyLinkClients";
        var expiryHours = int.TryParse(_configuration["Jwt:ExpiryHours"], out var hours) ? hours : 24;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(expiryHours),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
