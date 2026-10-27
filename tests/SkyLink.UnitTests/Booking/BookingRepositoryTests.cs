using BookingService.Data;
using BookingService.Models;
using BookingService.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace SkyLink.UnitTests.Booking;

[TestFixture]
public class BookingRepositoryTests
{
    private BookingDbContext _dbContext;
    private BookingRepository _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseInMemoryDatabase(databaseName: TestId.Next().ToString())
            .Options;

        _dbContext = new BookingDbContext(options);
        _repository = new BookingRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    public async Task TC_REPO_01_AddAsync_And_SaveChangesAsync_PersistsBooking()
    {
        // Arrange
        var booking = new BookingService.Models.Booking
        {
            BookingId = TestId.Next(),
            BookingReference = "AB12CD",
            UserId = TestId.Next(),
            FlightInstanceId = TestId.Next(),
            FareId = TestId.Next(),
            BookingStatus = "Confirmed",
            BaseFareTotal = 5000m,
            TaxTotal = 500m,
            GrandTotal = 5500m,
            CurrencyCode = "USD",
            BookedAtUtc = DateTime.UtcNow,
            Passengers = new List<Passenger>
            {
                new() { PassengerId = TestId.Next(), FirstName = "Alice", LastName = "Smith", DateOfBirth = new DateTime(1990, 1, 1), PassengerType = "Adult" }
            }
        };

        // Act
        await _repository.AddAsync(booking);
        await _repository.SaveChangesAsync();

        var retrieved = await _repository.GetByIdAsync(booking.BookingId);

        // Assert
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.BookingReference, Is.EqualTo("AB12CD"));
        Assert.That(retrieved.Passengers.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task TC_REPO_02_GetByIdAsync_IncludesPassengers()
    {
        // Arrange
        var booking = new BookingService.Models.Booking
        {
            BookingId = TestId.Next(),
            BookingReference = "CD34EF",
            UserId = TestId.Next(),
            FlightInstanceId = TestId.Next(),
            FareId = TestId.Next(),
            BookingStatus = "Confirmed",
            BaseFareTotal = 10000m,
            TaxTotal = 1000m,
            GrandTotal = 11000m,
            CurrencyCode = "USD",
            BookedAtUtc = DateTime.UtcNow,
            Passengers = new List<Passenger>
            {
                new() { PassengerId = TestId.Next(), FirstName = "Bob", LastName = "Smith", DateOfBirth = new DateTime(1985, 2, 2), PassengerType = "Adult" },
                new() { PassengerId = TestId.Next(), FirstName = "Charlie", LastName = "Smith", DateOfBirth = new DateTime(2015, 3, 3), PassengerType = "Child" }
            }
        };

        await _repository.AddAsync(booking);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(booking.BookingId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Passengers.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task TC_REPO_03_GetByUserIdAsync_FiltersByUser()
    {
        // Arrange
        var user1Id = TestId.Next();
        var user2Id = TestId.Next();

        var b1 = new BookingService.Models.Booking
        {
            BookingId = TestId.Next(),
            BookingReference = "U1B101",
            UserId = user1Id,
            FlightInstanceId = TestId.Next(),
            FareId = TestId.Next(),
            BookingStatus = "Confirmed",
            CurrencyCode = "USD",
            BookedAtUtc = DateTime.UtcNow
        };

        var b2 = new BookingService.Models.Booking
        {
            BookingId = TestId.Next(),
            BookingReference = "U2B202",
            UserId = user2Id,
            FlightInstanceId = TestId.Next(),
            FareId = TestId.Next(),
            BookingStatus = "Confirmed",
            CurrencyCode = "USD",
            BookedAtUtc = DateTime.UtcNow
        };

        await _repository.AddAsync(b1);
        await _repository.AddAsync(b2);
        await _repository.SaveChangesAsync();

        // Act
        var user1Bookings = (await _repository.GetByUserIdAsync(user1Id)).ToList();

        // Assert
        Assert.That(user1Bookings.Count, Is.EqualTo(1));
        Assert.That(user1Bookings[0].BookingReference, Is.EqualTo("U1B101"));
    }

    [Test]
    public async Task TC_REPO_06_GetByIdAsync_MissingId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(TestId.Next());

        // Assert
        Assert.That(result, Is.Null);
    }
}
