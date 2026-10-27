using CheckInService.Models;
using Microsoft.EntityFrameworkCore;

namespace CheckInService.Data;

public class CheckInDbContext : DbContext
{
    public CheckInDbContext(DbContextOptions<CheckInDbContext> options) : base(options)
    {
    }

    public DbSet<FlightSeatInventory> SeatInventories => Set<FlightSeatInventory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FlightSeatInventory>(entity =>
        {
            entity.ToTable("FlightSeatInventory");
            entity.HasKey(e => e.SeatId);
            entity.Property(e => e.SeatId).HasDefaultValueSql("NEXT VALUE FOR [dbo].[SeatIdSequence]");
            entity.Property(e => e.FlightInstanceId).IsRequired();
            entity.Property(e => e.SeatNumber).HasMaxLength(5).IsRequired().IsUnicode(false);
            entity.Property(e => e.SeatClass).HasMaxLength(20).IsRequired().IsUnicode(false);
            entity.Property(e => e.SeatStatus).HasMaxLength(20).IsRequired().IsUnicode(false);
            entity.Property(e => e.UpdatedAtUtc).IsRequired();

            entity.HasIndex(e => new { e.FlightInstanceId, e.SeatNumber }).IsUnique();
            entity.HasIndex(e => new { e.BookingId, e.PassengerId });
        });
    }
}
