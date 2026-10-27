using IdentityService.DTOs;
using IdentityService.Exceptions;
using IdentityService.Models;
using IdentityService.Repositories.Interfaces;
using IdentityService.Services.Interfaces;

namespace IdentityService.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly JwtService _jwtService;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        JwtService jwtService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new ArgumentException("Valid email is required");
        }

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
        {
            throw new InvalidPasswordException("Password must be at least 6 characters long.");
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (existingUser != null)
        {
            throw new UserAlreadyExistsException("Email already registered");
        }

        var firstName = dto.FirstName?.Trim() ?? string.Empty;
        var lastName = dto.LastName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(firstName))
        {
            firstName = "User";
        }

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = normalizedEmail,
            PhoneNumber = dto.PhoneNumber?.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Status = "Active"
        };

        // Public registration assigns "User" role only as per LLD
        var userRole = await _roleRepository.GetByNameAsync("User");
        if (userRole != null)
        {
            user.UserRoles.Add(new UserRole
            {
                UserId = user.UserId,
                RoleId = userRole.RoleId,
                Role = userRole
            });
        }

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();
        if (roles.Count == 0) roles.Add("User");

        var token = _jwtService.GenerateToken(user, roles);

        return new AuthResponseDto
        {
            UserId = user.UserId,
            Email = user.Email,
            Role = roles.First(),
            Roles = roles,
            Token = token,
            Message = "User registered successfully"
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new InvalidCredentialsException("Invalid email or password");
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);

        if (user == null)
        {
            throw new InvalidCredentialsException("Invalid email or password");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException("Invalid email or password");
        }

        if (user.Status != "Active")
        {
            throw new InvalidCredentialsException("Account is inactive or locked");
        }

        var roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();
        if (roles.Count == 0) roles.Add("User");

        var token = _jwtService.GenerateToken(user, roles);

        return new AuthResponseDto
        {
            UserId = user.UserId,
            Email = user.Email,
            Role = roles.First(),
            Roles = roles,
            Token = token,
            Message = "Login successful"
        };
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId);
        if (user == null)
        {
            throw new UserNotFoundException("User not found");
        }

        return new UserProfileDto
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Status = user.Status,
            Roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList()
        };
    }

    public async Task UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new UserNotFoundException("User not found");
        }

        if (!string.IsNullOrWhiteSpace(dto.FirstName))
        {
            user.FirstName = dto.FirstName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.LastName))
        {
            user.LastName = dto.LastName.Trim();
        }

        if (dto.PhoneNumber != null)
        {
            user.PhoneNumber = dto.PhoneNumber.Trim();
        }

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new UserNotFoundException("User not found");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidCredentialsException("Current password does not match");
        }

        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
        {
            throw new InvalidPasswordException("New password must be at least 6 characters long.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
    }
}
