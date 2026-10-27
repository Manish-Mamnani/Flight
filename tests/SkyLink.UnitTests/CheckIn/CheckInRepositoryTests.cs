using CheckInService.Data;
using CheckInService.Models;
using CheckInService.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace SkyLink.UnitTests.CheckIn;

[TestFixture]
public class CheckInRepositoryTests
{
    private CheckInDbContext _dbContext;
    private CheckInRepository _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<CheckInDbContext>()
            .UseInMemoryDatabase(databaseName: TestId.Next().ToString())
            .Options;

        _dbContext = new CheckInDbContext(options);
        _repository = new CheckInRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    public async Task TC_REPO_05_CheckInRepository_AddAndGetSeat_PersistsCorrectly()
    {
        // Arrange
        var flightInstanceId = TestId.Next();
        var seat = new FlightSeatInventory
        {
            SeatId = TestId.Next(),
            FlightInstanceId = flightInstanceId,
            SeatNumber = "12A",
            SeatClass = "Economy",
            SeatStatus = "Assigned",
            BookingId = TestId.Next(),
            PassengerId = TestId.Next(),
            UpdatedAtUtc = DateTime.UtcNow
        };

        // Act
        await _repository.AddAsync(seat);
        await _repository.SaveChangesAsync();

        var retrieved = await _repository.GetSeatAsync(flightInstanceId, "12A");

        // Assert
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.SeatNumber, Is.EqualTo("12A"));
        Assert.That(retrieved.SeatStatus, Is.EqualTo("Assigned"));
    }
}
