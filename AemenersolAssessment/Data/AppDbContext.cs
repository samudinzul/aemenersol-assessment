using Microsoft.EntityFrameworkCore;
using AemenersolAssessment.Models;

namespace AemenersolAssessment.Data;

public class AppDbContext : DbContext
{
    public DbSet<Platform> Platforms => Set<Platform>();
    public DbSet<Well> Wells => Set<Well>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        var provider = Environment.GetEnvironmentVariable("DB_PROVIDER") ?? "SqlServer";

        if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlite(connectionString ?? "Data Source=platformwell.db");
        }
        else
        {
            optionsBuilder.UseSqlServer(
                connectionString
                ?? @"Server=(localdb)\mssqllocaldb;Database=PlatformWellDb;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Table names match the assessment wording ("platform" / "well" tables).
        modelBuilder.Entity<Platform>().ToTable("platform");
        modelBuilder.Entity<Well>().ToTable("well");

        // Ids come from the API, so they must be inserted explicitly
        // (not generated as identity columns).
        modelBuilder.Entity<Platform>().Property(p => p.Id).ValueGeneratedNever();
        modelBuilder.Entity<Well>().Property(w => w.Id).ValueGeneratedNever();

        modelBuilder.Entity<Platform>()
            .HasMany(p => p.Wells)
            .WithOne(w => w.Platform)
            .HasForeignKey(w => w.PlatformId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Well>()
            .HasIndex(w => w.PlatformId);
    }
}
