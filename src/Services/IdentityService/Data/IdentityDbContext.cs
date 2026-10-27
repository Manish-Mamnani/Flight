using IdentityService.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Data;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).UseIdentityColumn();
            entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired().IsUnicode(false);
            entity.Property(e => e.LastName).HasMaxLength(50).IsRequired().IsUnicode(false);
            entity.Property(e => e.Email).HasMaxLength(250).IsRequired().IsUnicode(false);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired().IsUnicode(false);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15).IsUnicode(false);
            entity.Property(e => e.Status).HasMaxLength(30).IsRequired().HasDefaultValue("Active").IsUnicode(false);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.RoleId);
            entity.Property(e => e.RoleId).ValueGeneratedOnAdd();
            entity.Property(e => e.RoleName).HasMaxLength(20).IsRequired().IsUnicode(false);
            entity.HasIndex(e => e.RoleName).IsUnique();

            entity.HasData(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Manager" },
                new Role { RoleId = 3, RoleName = "User" }
            );
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(ur => ur.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(ur => ur.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
