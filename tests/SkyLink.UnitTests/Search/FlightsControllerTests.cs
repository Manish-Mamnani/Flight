using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SearchService.Controllers;
using SearchService.DTOs;
using SearchService.Exceptions;
using SearchService.Models;
using SearchService.Repositories.Interfaces;
using SearchService.Services;
using SearchService.Services.Interfaces;

namespace SkyLink.UnitTests.Search;

[TestFixture]
public class FlightsControllerTests
{
    private Mock<ISearchService> _searchServiceMock;
    private FlightsController _controller;

    [SetUp]
    public void SetUp()
    {
        _searchServiceMock = new Mock<ISearchService>();
        _controller = new FlightsController(_searchServiceMock.Object);
    }

    [Test]
    public async Task TC_SRCH_01_SearchFlights_MatchingRouteAndDate_Returns200OkWithFlights()
    {
        // Arrange
        var origin = "DEL";
        var destination = "BOM";
        var date = new DateTime(2026, 11, 10);
        var flights = new List<FlightResponseDto>
        {
            new() { FlightId = TestId.Next(), FlightNumber = "AI101", OriginAirportCode = "DEL", DestinationAirportCode = "BOM" }
        };

        _searchServiceMock.Setup(s => s.SearchAsync(origin, destination, date))
            .ReturnsAsync(flights);

        // Act
        var result = await _controller.SearchFlights(origin, destination, date);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        var returnedFlights = okResult.Value as IEnumerable<FlightResponseDto>;
        Assert.That(returnedFlights, Is.Not.Null);
        Assert.That(returnedFlights!.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task TC_SRCH_02_SearchFlights_UnservedRoute_Returns200OkWithEmptyCollection()
    {
        // Arrange
        var origin = "DEL";
        var destination = "XYZ";
        var date = new DateTime(2026, 11, 10);

        _searchServiceMock.Setup(s => s.SearchAsync(origin, destination, date))
            .ReturnsAsync(new List<FlightResponseDto>());

        // Act
        var result = await _controller.SearchFlights(origin, destination, date);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        var returnedFlights = okResult.Value as IEnumerable<FlightResponseDto>;
        Assert.That(returnedFlights, Is.Not.Null);
        Assert.That(returnedFlights!.Count(), Is.EqualTo(0));
    }

    [Test]
    public void TC_SRCH_03_SearchFlights_PastDate_ThrowsArgumentException()
    {
        // Arrange
        var origin = "DEL";
        var destination = "BOM";
        var pastDate = new DateTime(2020, 1, 1);

        _searchServiceMock.Setup(s => s.SearchAsync(origin, destination, pastDate))
            .ThrowsAsync(new ArgumentException("date cannot be in the past"));

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _controller.SearchFlights(origin, destination, pastDate));
    }

    [Test]
    public void TC_SRCH_04_SearchFlights_OriginEqualsDestination_ThrowsArgumentException()
    {
        // Arrange
        var samePlace = "DEL";
        var date = DateTime.UtcNow.AddDays(5);

        _searchServiceMock.Setup(s => s.SearchAsync(samePlace, samePlace, date))
            .ThrowsAsync(new ArgumentException("origin and destination must differ"));

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _controller.SearchFlights(samePlace, samePlace, date));
    }

    [Test]
    public void TC_SRCH_05_SearchFlights_MissingOrigin_ThrowsArgumentException()
    {
        // Arrange
        var date = DateTime.UtcNow.AddDays(5);
        _searchServiceMock.Setup(s => s.SearchAsync("", "BOM", date))
            .ThrowsAsync(new ArgumentException("origin is required"));

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _controller.SearchFlights("", "BOM", date));
    }

    [Test]
    public async Task TC_SRCH_06_GetFlight_ValidId_Returns200OkWithDetails()
    {
        // Arrange
        var flightId = TestId.Next();
        var flight = new FlightResponseDto { FlightId = flightId, FlightNumber = "6E202" };
        _searchServiceMock.Setup(s => s.GetFlightAsync(flightId)).ReturnsAsync(flight);

        // Act
        var result = await _controller.GetFlight(flightId);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.EqualTo(flight));
    }

    [Test]
    public void TC_SRCH_07_GetFlight_NonExistentId_ThrowsFlightNotFoundException()
    {
        // Arrange
        var unknownId = TestId.Next();
        _searchServiceMock.Setup(s => s.GetFlightAsync(unknownId))
            .ThrowsAsync(new FlightNotFoundException("Flight not found"));

        // Act & Assert
        Assert.ThrowsAsync<FlightNotFoundException>(async () => await _controller.GetFlight(unknownId));
    }

    [Test]
    public async Task TC_SRCH_08_RepositoryAbstraction_ServiceUsesIFlightRepository()
    {
        // Arrange
        var mockFlightRepo = new Mock<IFlightRepository>();
        var mockFlightInstanceRepo = new Mock<IFlightInstanceRepository>();
        var mockAirportRepo = new Mock<IAirportRepository>();
        var mockAirlineRepo = new Mock<IAirlineRepository>();

        var testDate = DateTime.UtcNow.AddDays(10);
        var origin = "DEL";
        var destination = "BOM";

        var fakeFlights = new List<Flight>
        {
            new() { FlightId = TestId.Next(), FlightNumber = "FL01", OriginAirport = new Airport { AirportCode = "DEL" }, DestinationAirport = new Airport { AirportCode = "BOM" }, Airline = new Airline { AirlineCode = "AI" } },
            new() { FlightId = TestId.Next(), FlightNumber = "FL02", OriginAirport = new Airport { AirportCode = "DEL" }, DestinationAirport = new Airport { AirportCode = "BOM" }, Airline = new Airline { AirlineCode = "6E" } }
        };

        mockFlightRepo.Setup(r => r.SearchFlightsAsync(origin, destination, testDate))
            .ReturnsAsync(fakeFlights);

        var service = new FlightSearchService(
            mockFlightRepo.Object,
            mockFlightInstanceRepo.Object,
            mockAirportRepo.Object,
            mockAirlineRepo.Object);

        // Act
        var result = await service.SearchAsync(origin, destination, testDate);

        // Assert
        Assert.That(result.Count(), Is.EqualTo(2));
        mockFlightRepo.Verify(r => r.SearchFlightsAsync(origin, destination, testDate), Times.Once);
    }
}
