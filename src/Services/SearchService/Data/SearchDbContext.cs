using Microsoft.EntityFrameworkCore;
using SearchService.Models;

namespace SearchService.Data;

public class SearchDbContext : DbContext
{
    public SearchDbContext(DbContextOptions<SearchDbContext> options) : base(options)
    {
    }

    public DbSet<Airport> Airports => Set<Airport>();
    public DbSet<Airline> Airlines => Set<Airline>();
    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<FlightInstance> FlightInstances => Set<FlightInstance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Airport>(entity =>
        {
            entity.ToTable("Airports");
            entity.HasKey(e => e.AirportId);
            entity.Property(e => e.AirportId).ValueGeneratedOnAdd();
            entity.Property(e => e.AirportCode).HasMaxLength(3).IsRequired().IsUnicode(false);
            entity.HasIndex(e => e.AirportCode).IsUnique();
            entity.Property(e => e.AirportName).HasMaxLength(150).IsRequired().IsUnicode(false);
            entity.Property(e => e.City).HasMaxLength(100).IsRequired().IsUnicode(false);
            entity.Property(e => e.Country).HasMaxLength(100).IsRequired().IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(30).IsRequired().HasDefaultValue("Active").IsUnicode(false);

            entity.HasData(
                new Airport { AirportId = 1, AirportCode = "DEL", AirportName = "Indira Gandhi International Airport", City = "New Delhi", Country = "India", Status = "Active" },
                new Airport { AirportId = 2, AirportCode = "BOM", AirportName = "Chhatrapati Shivaji Maharaj International Airport", City = "Mumbai", Country = "India", Status = "Active" },
                new Airport { AirportId = 3, AirportCode = "BLR", AirportName = "Kempegowda International Airport", City = "Bengaluru", Country = "India", Status = "Active" }
            );
        });

        modelBuilder.Entity<Airline>(entity =>
        {
            entity.ToTable("Airlines");
            entity.HasKey(e => e.AirlineId);
            entity.Property(e => e.AirlineId).ValueGeneratedOnAdd();
            entity.Property(e => e.AirlineCode).HasMaxLength(3).IsRequired().IsUnicode(false);
            entity.HasIndex(e => e.AirlineCode).IsUnique();
            entity.Property(e => e.AirlineName).HasMaxLength(150).IsRequired().IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(30).IsRequired().HasDefaultValue("Active").IsUnicode(false);

            entity.HasData(
                new Airline { AirlineId = 1, AirlineCode = "AI", AirlineName = "Air India", Status = "Active" },
                new Airline { AirlineId = 2, AirlineCode = "6E", AirlineName = "IndiGo", Status = "Active" }
            );
        });

        modelBuilder.Entity<Flight>(entity =>
        {
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Flight_Origin_Destination", "[OriginAirportId] <> [DestinationAirportId]");
            });

            entity.HasKey(e => e.FlightId);
            entity.Property(e => e.FlightId).HasDefaultValueSql("NEXT VALUE FOR [dbo].[FlightIdSequence]");
            entity.Property(e => e.FlightNumber).HasMaxLength(10).IsRequired().IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(30).IsRequired().HasDefaultValue("Active").IsUnicode(false);
            entity.Property(e => e.CreatedByUserId).IsRequired();

            entity.HasOne(f => f.Airline)
                  .WithMany(a => a.Flights)
                  .HasForeignKey(f => f.AirlineId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.OriginAirport)
                  .WithMany(a => a.DepartingFlights)
                  .HasForeignKey(f => f.OriginAirportId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.DestinationAirport)
                  .WithMany(a => a.ArrivingFlights)
                  .HasForeignKey(f => f.DestinationAirportId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FlightInstance>(entity =>
        {
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_FlightInstance_Arrival_Departure", "[ArrivalAtUtc] > [DepartureAtUtc]");
                t.HasCheckConstraint("CK_FlightInstance_TotalSeats_Positive", "[TotalSeats] > 0");
            });

            entity.HasKey(e => e.FlightInstanceId);
            entity.Property(e => e.FlightInstanceId).HasDefaultValueSql("NEXT VALUE FOR [dbo].[FlightInstanceIdSequence]");
            entity.Property(e => e.FlightStatus).HasMaxLength(20).IsRequired().IsUnicode(false);

            entity.HasOne(fi => fi.Flight)
                  .WithMany(f => f.FlightInstances)
                  .HasForeignKey(fi => fi.FlightId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
