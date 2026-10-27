using System.Security.Claims;
using BookingService.Controllers;
using BookingService.DTOs;
using BookingService.Exceptions;
using BookingService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace SkyLink.UnitTests.Booking;

[TestFixture]
public class BookingControllerTests
{
    private Mock<IBookingService> _bookingServiceMock;
    private BookingsController _controller;

    [SetUp]
    public void SetUp()
    {
        _bookingServiceMock = new Mock<IBookingService>();
        _controller = new BookingsController(_bookingServiceMock.Object);
    }

    [Test]
    public async Task TC_BKG_01_CreateBooking_ValidRequest_Returns201CreatedWithPnr()
    {
        // Arrange
        var userId = TestId.Next();
        SetUserContext(userId);

        var dto = new CreateBookingDto
        {
            FlightInstanceId = TestId.Next(),
            FareId = TestId.Next(),
            Passengers = new List<CreatePassengerDto>
            {
                new() { FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(1990, 1, 1), PassengerType = "Adult" }
            }
        };

        var response = new BookingResponseDto
        {
            BookingId = TestId.Next(),
            BookingReference = "SK123456",
            BookingStatus = "Confirmed",
            GrandTotal = 5000m,
            CurrencyCode = "USD"
        };

        _bookingServiceMock.Setup(s => s.CreateBookingAsync(dto, userId)).ReturnsAsync(response);

        // Act
        var result = await _controller.CreateBooking(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objResult = (ObjectResult)result;
        Assert.That(objResult.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
        Assert.That(objResult.Value, Is.EqualTo(response));
    }

    [Test]
    public async Task TC_BKG_02_CreateBooking_TotalAmountComputedCorrectly()
    {
        // Arrange
        var userId = TestId.Next();
        SetUserContext(userId);

        var dto = new CreateBookingDto
        {
            FlightInstanceId = TestId.Next(),
            FareId = TestId.Next(),
            Passengers = new List<CreatePassengerDto>
            {
                new() { FirstName = "Alice", LastName = "Smith", DateOfBirth = new DateTime(1992, 5, 10), PassengerType = "Adult" },
                new() { FirstName = "Bob", LastName = "Smith", DateOfBirth = new DateTime(1995, 8, 20), PassengerType = "Adult" }
            }
        };

        var response = new BookingResponseDto
        {
            BookingId = TestId.Next(),
            BookingReference = "SK987654",
            BookingStatus = "Confirmed",
            BaseFareTotal = 10000m,
            GrandTotal = 10000m
        };

        _bookingServiceMock.Setup(s => s.CreateBookingAsync(dto, userId)).ReturnsAsync(response);

        // Act
        var result = await _controller.CreateBooking(dto);

        // Assert
        var objResult = (ObjectResult)result;
        var val = (BookingResponseDto)objResult.Value!;
        Assert.That(val.GrandTotal, Is.EqualTo(10000m));
        _bookingServiceMock.Verify(s => s.CreateBookingAsync(dto, userId), Times.Once);
    }

    [Test]
    public void TC_BKG_03_CreateBooking_FlightDoesNotExist_ThrowsBookingNotFoundException()
    {
        // Arrange
        var userId = TestId.Next();
        SetUserContext(userId);

        var dto = new CreateBookingDto
        {
            FlightId = TestId.Next(),
            FareId = TestId.Next(),
            Passengers = new List<CreatePassengerDto>
            {
                new() { FirstName = "Alice", LastName = "Smith", DateOfBirth = new DateTime(1990, 1, 1), PassengerType = "Adult" }
            }
        };

        _bookingServiceMock.Setup(s => s.CreateBookingAsync(dto, userId))
            .ThrowsAsync(new BookingNotFoundException("Flight not found"));

        // Act & Assert
        Assert.ThrowsAsync<BookingNotFoundException>(async () => await _controller.CreateBooking(dto));
    }

    [Test]
    public void TC_BKG_04_CreateBooking_NoPassengersSupplied_ThrowsArgumentException()
    {
        // Arrange
        var userId = TestId.Next();
        SetUserContext(userId);

        var dto = new CreateBookingDto
        {
            FlightId = TestId.Next(),
            FareId = TestId.Next(),
            Passengers = new List<CreatePassengerDto>()
        };

        _bookingServiceMock.Setup(s => s.CreateBookingAsync(dto, userId))
            .ThrowsAsync(new ArgumentException("at least one passenger required"));

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await _controller.CreateBooking(dto));
    }

    [Test]
    public void TC_BKG_05_CreateBooking_WithoutLogin_ThrowsUnauthorized()
    {
        // Arrange - no user set
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var dto = new CreateBookingDto { FareId = TestId.Next() };

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _controller.CreateBooking(dto));
    }

    [Test]
    public async Task TC_BKG_06_GetBooking_OwnerViewsOwnBooking_Returns200OkWithDetails()
    {
        // Arrange
        var userId = TestId.Next();
        SetUserContext(userId);

        var reference = "SK123456";
        var expected = new BookingResponseDto { BookingReference = reference, UserId = userId };
        _bookingServiceMock.Setup(s => s.GetBookingAsync(reference, userId, false)).ReturnsAsync(expected);

        // Act
        var result = await _controller.GetByReference(reference);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        Assert.That(ok.Value, Is.EqualTo(expected));
    }

    [Test]
    public void TC_BKG_07_GetBooking_UserViewsAnotherUserBooking_ThrowsUnauthorizedOrForbidden()
    {
        // Arrange
        var user2Id = TestId.Next();
        SetUserContext(user2Id);

        var reference = "SK123456";
        _bookingServiceMock.Setup(s => s.GetBookingAsync(reference, user2Id, false))
            .ThrowsAsync(new UnauthorizedAccessException("You do not have permission to view this booking."));

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _controller.GetByReference(reference));
    }

    [Test]
    public void TC_BKG_08_GetBooking_BookingNotFound_ThrowsBookingNotFoundException()
    {
        // Arrange
        var userId = TestId.Next();
        SetUserContext(userId);

        var randomRef = TestId.Next().ToString();
        _bookingServiceMock.Setup(s => s.GetBookingAsync(randomRef, userId, false))
            .ThrowsAsync(new BookingNotFoundException("Booking not found"));

        // Act & Assert
        Assert.ThrowsAsync<BookingNotFoundException>(async () => await _controller.GetByReference(randomRef));
    }

    [Test]
    public async Task TC_BKG_09_CancelBooking_OwnerCancelsConfirmedBooking_Returns200Ok()
    {
        // Arrange
        var userId = TestId.Next();
        SetUserContext(userId);

        var reference = "SK123456";
        var response = new BookingResponseDto { BookingReference = reference, BookingStatus = "Cancelled" };
        _bookingServiceMock.Setup(s => s.CancelBookingAsync(reference, userId, false)).ReturnsAsync(response);

        // Act
        var result = await _controller.CancelBooking(reference);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        var b = (BookingResponseDto)ok.Value!;
        Assert.That(b.BookingStatus, Is.EqualTo("Cancelled"));
    }

    [Test]
    public void TC_BKG_10_CancelBooking_CancelAlreadyCancelled_ThrowsInvalidBookingOperationException()
    {
        // Arrange
        var userId = TestId.Next();
        SetUserContext(userId);

        var reference = "SK123456";
        _bookingServiceMock.Setup(s => s.CancelBookingAsync(reference, userId, false))
            .ThrowsAsync(new InvalidBookingOperationException("Booking is already cancelled."));

        // Act & Assert
        Assert.ThrowsAsync<InvalidBookingOperationException>(async () => await _controller.CancelBooking(reference));
    }

    [Test]
    public void TC_BKG_11_CancelBooking_NonOwnerCancels_ThrowsUnauthorized()
    {
        // Arrange
        var user2Id = TestId.Next();
        SetUserContext(user2Id);

        var reference = "SK123456";
        _bookingServiceMock.Setup(s => s.CancelBookingAsync(reference, user2Id, false))
            .ThrowsAsync(new UnauthorizedAccessException("You do not have permission to cancel this booking."));

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _controller.CancelBooking(reference));
    }

    private void SetUserContext(int userId, string role = "User")
    {
        var claims = new List<Claim>
        {
            new("UserId", userId.ToString()),
            new(ClaimTypes.Role, role)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }
}
