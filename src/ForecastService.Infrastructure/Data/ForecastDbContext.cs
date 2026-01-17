using ForecastService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ForecastService.Infrastructure.Data;

public class ForecastDbContext(DbContextOptions<ForecastDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies { get; set; }
    public DbSet<PowerPlant> PowerPlants { get; set; }
    public DbSet<Forecast> Forecasts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Company configuration
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        // PowerPlant configuration
        modelBuilder.Entity<PowerPlant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(e => e.Company)
                .WithMany(c => c.PowerPlants)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.CompanyId, e.Country });
        });

        // Forecast configuration
        modelBuilder.Entity<Forecast>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ForecastDateTime).IsRequired();
            entity.Property(e => e.ProductionMWh).IsRequired().HasPrecision(18, 6);
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(e => e.PowerPlant)
                .WithMany(p => p.Forecasts)
                .HasForeignKey(e => e.PowerPlantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.PowerPlantId, e.ForecastDateTime, e.IsActive });
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var companyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Seed Company
        modelBuilder.Entity<Company>().HasData(new Company
        {
            Id = companyId,
            Name = "Energy Trading Corp",
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        // Seed Power Plants
        var turkeyPlantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var bulgariaPlantId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var spainPlantId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        modelBuilder.Entity<PowerPlant>().HasData(
            new PowerPlant
            {
                Id = turkeyPlantId,
                Name = "Turkey Power Plant",
                Country = "Turkey",
                CompanyId = companyId,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new PowerPlant
            {
                Id = bulgariaPlantId,
                Name = "Bulgaria Power Plant",
                Country = "Bulgaria",
                CompanyId = companyId,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new PowerPlant
            {
                Id = spainPlantId,
                Name = "Spain Power Plant",
                Country = "Spain",
                CompanyId = companyId,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        );
    }
}
