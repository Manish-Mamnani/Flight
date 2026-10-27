using CheckInService.Controllers;
using CheckInService.DTOs;
using CheckInService.Exceptions;
using CheckInService.HttpClients;
using CheckInService.Models;
using CheckInService.Repositories.Interfaces;
using CheckInService.Services;
using CheckInService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace SkyLink.UnitTests.CheckIn;

[TestFixture]
public class CheckInControllerTests
{
    private Mock<ICheckInService> _checkInServiceMock;
    private CheckInController _controller;

    [SetUp]
    public void SetUp()
    {
        _checkInServiceMock = new Mock<ICheckInService>();
        _controller = new CheckInController(_checkInServiceMock.Object);
    }

    [Test]
    public async Task TC_CHK_01_CheckIn_ValidCheckInInsideWindow_Returns200WithCheckedInStatus()
    {
        // Arrange
        var request = new CheckInRequestDto
        {
            BookingId = TestId.Next(),
            PassengerId = TestId.Next(),
            SeatNumber = "12A"
        };
        var expected = new CheckInResponseDto
        {
            SeatId = TestId.Next(),
            PassengerId = request.PassengerId,
            SeatNumber = "12A",
            Status = "CHECKED_IN"
        };
        _checkInServiceMock.Setup(s => s.CheckInAsync(request)).ReturnsAsync(expected);

        // Act
        var result = await _controller.DirectCheckIn(request);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objResult = (ObjectResult)result;
        Assert.That(objResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objResult.Value, Is.EqualTo(expected));
    }

    [Test]
    public void TC_CHK_02_CheckIn_TooEarlyOrLate_ThrowsCheckInWindowClosedException()
    {
        // Arrange
        var request = new CheckInRequestDto
        {
            BookingId = TestId.Next(),
            PassengerId = TestId.Next(),
            SeatNumber = "12A"
        };
        _checkInServiceMock.Setup(s => s.CheckInAsync(request))
            .ThrowsAsync(new CheckInWindowClosedException("check-in not open"));

        // Act & Assert
        Assert.ThrowsAsync<CheckInWindowClosedException>(async () => await _controller.DirectCheckIn(request));
    }

    [Test]
    public void TC_CHK_03_CheckIn_CancelledBooking_ThrowsCheckInException()
    {
        // Arrange
        var request = new CheckInRequestDto
        {
            BookingId = TestId.Next(),
            PassengerId = TestId.Next(),
            SeatNumber = "12A"
        };
        _checkInServiceMock.Setup(s => s.CheckInAsync(request))
            .ThrowsAsync(new CheckInException("booking is not active"));

        // Act & Assert
        Assert.ThrowsAsync<CheckInException>(async () => await _controller.DirectCheckIn(request));
    }

    [Test]
    public async Task TC_CHK_04_SeatSelection_SelectAvailableSeat_AssignsSeat()
    {
        // Arrange
        var request = new CheckInRequestDto
        {
            BookingId = TestId.Next(),
            PassengerId = TestId.Next(),
            SeatNumber = "12A"
        };
        var response = new CheckInResponseDto
        {
            SeatNumber = "12A",
            PassengerId = request.PassengerId,
            Status = "CHECKED_IN"
        };
        _checkInServiceMock.Setup(s => s.CheckInAsync(request)).ReturnsAsync(response);

        // Act
        var result = await _controller.DirectCheckIn(request);

        // Assert
        var ok = (ObjectResult)result;
        var dto = (CheckInResponseDto)ok.Value!;
        Assert.That(dto.SeatNumber, Is.EqualTo("12A"));
        Assert.That(dto.Status, Is.EqualTo("CHECKED_IN"));
    }

    [Test]
    public void TC_CHK_05_SeatSelection_SelectSeatAlreadyTaken_ThrowsSeatAlreadyAssignedException()
    {
        // Arrange
        var request = new CheckInRequestDto
        {
            BookingId = TestId.Next(),
            PassengerId = TestId.Next(),
            SeatNumber = "12A"
        };
        _checkInServiceMock.Setup(s => s.CheckInAsync(request))
            .ThrowsAsync(new SeatAlreadyAssignedException("seat already assigned"));

        // Act & Assert
        Assert.ThrowsAsync<SeatAlreadyAssignedException>(async () => await _controller.DirectCheckIn(request));
    }

    [Test]
    public async Task TC_CHK_06_SeatSelection_SeatAssignedOnlyAfterAvailabilityCheck()
    {
        // Arrange
        var mockRepo = new Mock<ICheckInRepository>();
        var mockBookingClient = new Mock<IBookingServiceClient>();
        var mockSearchClient = new Mock<ISearchServiceClient>();

        var instanceId = TestId.Next();
        var passengerId = TestId.Next();
        var bookingId = TestId.Next();

        mockBookingClient.Setup(b => b.GetBookingValidationAsync(It.IsAny<string>()))
            .ReturnsAsync(new BookingDetailsResponseDto
            {
                BookingId = bookingId,
                FlightInstanceId = instanceId,
                BookingStatus = "Confirmed",
                PassengerIds = new List<int> { passengerId }
            });

        mockSearchClient.Setup(s => s.GetFlightInstanceAsync(instanceId))
            .ReturnsAsync(new FlightDetailsResponseDto
            {
                FlightInstanceId = instanceId,
                DepartureAtUtc = DateTime.UtcNow.AddHours(5) // within 48h
            });

        mockRepo.Setup(r => r.GetAssignedSeatForPassengerAsync(instanceId, passengerId))
            .ReturnsAsync((FlightSeatInventory?)null);

        mockRepo.Setup(r => r.GetSeatAsync(instanceId, "12A"))
            .ReturnsAsync((FlightSeatInventory?)null);

        var service = new CheckInServiceImpl(mockRepo.Object, mockBookingClient.Object, mockSearchClient.Object);

        var request = new CheckInRequestDto
        {
            BookingId = bookingId,
            PassengerId = passengerId,
            SeatNumber = "12A"
        };

        // Act
        var result = await service.CheckInAsync(request);

        // Assert
        Assert.That(result.SeatNumber, Is.EqualTo("12A"));
        mockRepo.Verify(r => r.GetSeatAsync(instanceId, "12A"), Times.Once);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<FlightSeatInventory>()), Times.Once);
    }

    [Test]
    public async Task TC_CHK_07_CheckIn_RepeatCheckInForSamePassenger_ReturnsExistingCheckIn()
    {
        // Arrange
        var request = new CheckInRequestDto
        {
            BookingId = TestId.Next(),
            PassengerId = TestId.Next(),
            SeatNumber = "12A"
        };
        var existing = new CheckInResponseDto
        {
            SeatNumber = "12A",
            PassengerId = request.PassengerId,
            Status = "CHECKED_IN",
            Message = "Passenger is already checked in."
        };
        _checkInServiceMock.Setup(s => s.CheckInAsync(request)).ReturnsAsync(existing);

        // Act
        var result = await _controller.DirectCheckIn(request);

        // Assert
        var ok = (ObjectResult)result;
        var val = (CheckInResponseDto)ok.Value!;
        Assert.That(val.Status, Is.EqualTo("CHECKED_IN"));
    }

    [Test]
    public void TC_CHK_08_CheckIn_PassengerDoesNotBelongToBooking_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new CheckInRequestDto
        {
            BookingId = TestId.Next(),
            PassengerId = TestId.Next(),
            SeatNumber = "12A"
        };
        _checkInServiceMock.Setup(s => s.CheckInAsync(request))
            .ThrowsAsync(new KeyNotFoundException("passenger not found on booking"));

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () => await _controller.DirectCheckIn(request));
    }
}
