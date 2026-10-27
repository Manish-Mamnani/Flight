using FareService.Exceptions;
using FareService.HttpClients;
using FareService.Models;
using FareService.Repositories.Interfaces;
using FareService.Services;
using Moq;
using NUnit.Framework;

namespace SkyLink.UnitTests.Fare;

[TestFixture]
public class FareServiceTests
{
    private Mock<IFareRepository> _fareRepoMock;
    private Mock<ISearchServiceClient> _searchClientMock;
    private FareServiceImpl _fareService;

    [SetUp]
    public void SetUp()
    {
        _fareRepoMock = new Mock<IFareRepository>();
        _searchClientMock = new Mock<ISearchServiceClient>();
        _fareService = new FareServiceImpl(_fareRepoMock.Object, _searchClientMock.Object);
    }

    [Test]
    public async Task TC_FARE_01_FareQuote_KnownRouteAndEconomyClass_Returns200WithExpectedAmount()
    {
        // Arrange
        var flightInstanceId = TestId.Next();
        var fare = new FareService.Models.Fare
        {
            FareId = TestId.Next(),
            FlightInstanceId = flightInstanceId,
            FareClass = "Economy",
            BaseFare = 4000m,
            TaxAmount = 500m,
            CurrencyCode = "USD"
        };

        _fareRepoMock.Setup(r => r.GetByInstanceAndClassAsync(flightInstanceId, "Economy"))
            .ReturnsAsync(fare);

        // Act
        var quote = await _fareService.GetFareQuoteAsync(null, flightInstanceId, "Economy", 1);

        // Assert
        Assert.That(quote.TotalAmount, Is.EqualTo(4500m));
        Assert.That(quote.Currency, Is.EqualTo("USD"));
        Assert.That(quote.FareClass, Is.EqualTo("Economy"));
    }

    [Test]
    public async Task TC_FARE_02_FareQuote_BusinessClassPricedHigherThanEconomy()
    {
        // Arrange
        var flightInstanceId = TestId.Next();
        var economyFare = new FareService.Models.Fare
        {
            FlightInstanceId = flightInstanceId,
            FareClass = "Economy",
            BaseFare = 5000m,
            TaxAmount = 500m,
            CurrencyCode = "USD"
        };
        var businessFare = new FareService.Models.Fare
        {
            FlightInstanceId = flightInstanceId,
            FareClass = "Business",
            BaseFare = 12000m,
            TaxAmount = 1500m,
            CurrencyCode = "USD"
        };

        _fareRepoMock.Setup(r => r.GetByInstanceAndClassAsync(flightInstanceId, "Economy"))
            .ReturnsAsync(economyFare);
        _fareRepoMock.Setup(r => r.GetByInstanceAndClassAsync(flightInstanceId, "Business"))
            .ReturnsAsync(businessFare);

        // Act
        var ecoQuote = await _fareService.GetFareQuoteAsync(null, flightInstanceId, "Economy", 1);
        var busQuote = await _fareService.GetFareQuoteAsync(null, flightInstanceId, "Business", 1);

        // Assert
        Assert.That(busQuote.TotalAmount, Is.GreaterThan(ecoQuote.TotalAmount));
    }

    [Test]
    public void TC_FARE_03_FareQuote_InvalidCabinClass_ThrowsArgumentException()
    {
        // Arrange
        var flightId = TestId.Next();

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _fareService.GetFareQuoteAsync(flightId, null, "Luxury", 1));

        Assert.That(ex!.Message, Does.Contain("invalid cabin class"));
    }

    [Test]
    public void TC_FARE_04_FareQuote_MissingFlightId_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _fareService.GetFareQuoteAsync(null, null, "Economy", 1));

        Assert.That(ex!.Message, Does.Contain("flightId is required"));
    }

    [Test]
    public async Task TC_FARE_05_FareQuote_TotalForMultiplePassengers_ComputesCorrectly()
    {
        // Arrange
        var flightInstanceId = TestId.Next();
        var fare = new FareService.Models.Fare
        {
            FlightInstanceId = flightInstanceId,
            FareClass = "Economy",
            BaseFare = 3000m,
            TaxAmount = 500m,
            CurrencyCode = "INR"
        };

        _fareRepoMock.Setup(r => r.GetByInstanceAndClassAsync(flightInstanceId, "Economy"))
            .ReturnsAsync(fare);

        // Act
        var quote = await _fareService.GetFareQuoteAsync(null, flightInstanceId, "Economy", 3);

        // Assert
        Assert.That(quote.TotalAmount, Is.EqualTo(3500m * 3));
        Assert.That(quote.Currency, Is.EqualTo("INR"));
    }

    [Test]
    public void TC_FARE_06_FareQuote_ZeroPassengers_ThrowsArgumentException()
    {
        // Arrange
        var flightId = TestId.Next();

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _fareService.GetFareQuoteAsync(flightId, null, "Economy", 0));

        Assert.That(ex!.Message, Does.Contain("at least one passenger required"));
    }

    [Test]
    public void TC_FARE_07_FareQuote_NoFareRuleForRoute_ThrowsFareNotFoundException()
    {
        // Arrange
        var flightInstanceId = TestId.Next();
        _fareRepoMock.Setup(r => r.GetByInstanceAndClassAsync(flightInstanceId, "Economy"))
            .ReturnsAsync((FareService.Models.Fare?)null);
        _fareRepoMock.Setup(r => r.GetByFlightInstanceIdAsync(flightInstanceId))
            .ReturnsAsync(new List<FareService.Models.Fare>());

        // Act & Assert
        Assert.ThrowsAsync<FareNotFoundException>(async () =>
            await _fareService.GetFareQuoteAsync(null, flightInstanceId, "Economy", 1));
    }
}
