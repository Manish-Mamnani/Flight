using FareService.Models;
using Microsoft.EntityFrameworkCore;

namespace FareService.Data;

public class FareDbContext : DbContext
{
    public FareDbContext(DbContextOptions<FareDbContext> options) : base(options)
    {
    }

    public DbSet<Fare> Fares => Set<Fare>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Fare>(entity =>
        {
            entity.ToTable("Fares", t =>
            {
                t.HasCheckConstraint("CK_Fares_BaseFare_NonNegative", "[BaseFare] >= 0");
                t.HasCheckConstraint("CK_Fares_TaxAmount_NonNegative", "[TaxAmount] >= 0");
            });

            entity.HasKey(e => e.FareId);
            entity.Property(e => e.FareId).HasDefaultValueSql("NEXT VALUE FOR [dbo].[FareIdSequence]");
            entity.Property(e => e.FlightInstanceId).IsRequired();
            entity.Property(e => e.FareClass).HasMaxLength(20).IsRequired().IsUnicode(false);
            entity.Property(e => e.BaseFare).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.CurrencyCode).HasMaxLength(3).IsFixedLength().IsRequired().IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(30).IsRequired().HasDefaultValue("Active").IsUnicode(false);

            entity.HasIndex(e => new { e.FlightInstanceId, e.FareClass, e.Status });
        });
    }
}
