using System.Security.Claims;
using IdentityService.Controllers;
using IdentityService.DTOs;
using IdentityService.Exceptions;
using IdentityService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace SkyLink.UnitTests.Auth;

[TestFixture]
public class AuthControllerTests
{
    private Mock<IAuthService> _authServiceMock;
    private AuthController _controller;

    [SetUp]
    public void SetUp()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Test]
    public async Task TC_AUTH_01_Register_WithValidData_Returns201Created()
    {
        // Arrange
        var dto = new RegisterDto
        {
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@mail.com",
            Password = "Pass@1234"
        };
        var expectedResponse = new AuthResponseDto
        {
            UserId = TestId.Next(),
            Email = dto.Email,
            Role = "User",
            Token = "valid.jwt.token"
        };
        _authServiceMock.Setup(s => s.RegisterAsync(dto)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Register(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
        Assert.That(objectResult.Value, Is.EqualTo(expectedResponse));
    }

    [Test]
    public void TC_AUTH_02_Register_DuplicateEmail_ThrowsUserAlreadyExistsException()
    {
        // Arrange
        var dto = new RegisterDto { Email = "alice@mail.com", Password = "Pass@1234" };
        _authServiceMock.Setup(s => s.RegisterAsync(dto))
            .ThrowsAsync(new UserAlreadyExistsException("Email already registered"));

        // Act & Assert
        Assert.ThrowsAsync<UserAlreadyExistsException>(async () => await _controller.Register(dto));
    }

    [Test]
    public async Task TC_AUTH_03_Register_EmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var dto = new RegisterDto { Email = "", Password = "Pass@1234" };
        _controller.ModelState.AddModelError("Email", "valid email is required");

        // Act
        var result = await _controller.Register(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void TC_AUTH_04_Register_WeakPassword_ThrowsInvalidPasswordException()
    {
        // Arrange
        var dto = new RegisterDto { Email = "alice@mail.com", Password = "abc" };
        _authServiceMock.Setup(s => s.RegisterAsync(dto))
            .ThrowsAsync(new InvalidPasswordException("password policy violated"));

        // Act & Assert
        Assert.ThrowsAsync<InvalidPasswordException>(async () => await _controller.Register(dto));
    }

    [Test]
    public async Task TC_AUTH_05_Login_ValidCredentials_Returns200OkWithToken()
    {
        // Arrange
        var dto = new LoginDto { Email = "alice@mail.com", Password = "Pass@1234" };
        var authResponse = new AuthResponseDto
        {
            UserId = TestId.Next(),
            Email = dto.Email,
            Role = "User",
            Token = "jwt.token.string"
        };
        _authServiceMock.Setup(s => s.LoginAsync(dto)).ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Login(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(okResult.Value, Is.EqualTo(authResponse));
    }

    [Test]
    public void TC_AUTH_06_Login_WrongPassword_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var dto = new LoginDto { Email = "alice@mail.com", Password = "wrong" };
        _authServiceMock.Setup(s => s.LoginAsync(dto))
            .ThrowsAsync(new InvalidCredentialsException("invalid email or password"));

        // Act & Assert
        Assert.ThrowsAsync<InvalidCredentialsException>(async () => await _controller.Login(dto));
    }

    [Test]
    public void TC_AUTH_07_Login_UnregisteredEmail_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var dto = new LoginDto { Email = "nobody@mail.com", Password = "Password123!" };
        _authServiceMock.Setup(s => s.LoginAsync(dto))
            .ThrowsAsync(new InvalidCredentialsException("invalid email or password"));

        // Act & Assert
        Assert.ThrowsAsync<InvalidCredentialsException>(async () => await _controller.Login(dto));
    }

    [Test]
    public void TC_AUTH_08_AccessProtectedApiWithoutToken_ThrowsUnauthorized()
    {
        // Arrange - user context without identity
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _controller.Me());
    }

    [Test]
    public async Task TC_AUTH_09_AccessProtectedApiWithValidToken_ReturnsUserProfile()
    {
        // Arrange
        var userId = TestId.Next();
        var claims = new List<Claim>
        {
            new("UserId", userId.ToString()),
            new(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var profile = new UserProfileDto { UserId = userId, Email = "test@mail.com" };
        _authServiceMock.Setup(s => s.GetProfileAsync(userId)).ReturnsAsync(profile);

        // Act
        var result = await _controller.Me();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task TC_AUTH_10_UpdateProfile_WhenCalled_ReturnsSuccess()
    {
        // Arrange
        var userId = TestId.Next();
        var claims = new List<Claim> { new("UserId", userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var dto = new UpdateProfileDto { FirstName = "Alice", LastName = "Wonderland" };
        _authServiceMock.Setup(s => s.UpdateProfileAsync(userId, dto)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateProfile(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }
}
