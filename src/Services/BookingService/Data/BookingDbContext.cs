using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Data;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
    {
    }

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Passenger> Passengers => Set<Passenger>();
    public DbSet<BookingStatusHistory> BookingStatusHistories => Set<BookingStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("Bookings");
            entity.HasKey(e => e.BookingId);
            entity.Property(e => e.BookingId).HasDefaultValueSql("NEXT VALUE FOR [dbo].[BookingIdSequence]");
            entity.Property(e => e.BookingReference).HasMaxLength(12).IsRequired().IsUnicode(false);
            entity.HasIndex(e => e.BookingReference).IsUnique();
            entity.Property(e => e.BookingStatus).HasMaxLength(20).IsRequired().IsUnicode(false);
            entity.Property(e => e.BaseFareTotal).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.TaxTotal).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.GrandTotal).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.CurrencyCode).HasMaxLength(3).IsFixedLength().IsRequired().IsUnicode(false);
            entity.Property(e => e.BookedAtUtc).IsRequired();
            entity.Property(e => e.RowVersion).IsRowVersion();

            entity.HasIndex(e => new { e.UserId, e.BookedAtUtc });
        });

        modelBuilder.Entity<Passenger>(entity =>
        {
            entity.ToTable("Passengers");
            entity.HasKey(e => e.PassengerId);
            entity.Property(e => e.PassengerId).HasDefaultValueSql("NEXT VALUE FOR [dbo].[PassengerIdSequence]");
            entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired().IsUnicode(false);
            entity.Property(e => e.LastName).HasMaxLength(50).IsRequired().IsUnicode(false);
            entity.Property(e => e.DateOfBirth).HasColumnType("date").IsRequired();
            entity.Property(e => e.Gender).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.PassengerType).HasMaxLength(20).IsRequired().IsUnicode(false);
            entity.Property(e => e.PassportNumber).HasMaxLength(30).IsUnicode(false);

            entity.HasOne(p => p.Booking)
                  .WithMany(b => b.Passengers)
                  .HasForeignKey(p => p.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookingStatusHistory>(entity =>
        {
            entity.ToTable("BookingStatusHistory");
            entity.HasKey(e => e.HistoryId);
            entity.Property(e => e.HistoryId).ValueGeneratedOnAdd();
            entity.Property(e => e.PreviousStatus).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.NewStatus).HasMaxLength(20).IsRequired().IsUnicode(false);
            entity.Property(e => e.Remarks).HasMaxLength(250).IsUnicode(false);
            entity.Property(e => e.ChangedAtUtc).IsRequired();

            entity.HasOne(h => h.Booking)
                  .WithMany(b => b.StatusHistory)
                  .HasForeignKey(h => h.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.BookingId, e.ChangedAtUtc });
        });
    }
}
