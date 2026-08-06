using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using NuclearDomain.Entities;
using NuclearDomain.Entities.Telemetries;

namespace NuclearInfrastructure.Repositories;

public class ReactorDbContext : DbContext
{
    public ReactorDbContext(DbContextOptions<ReactorDbContext> options) : base(options) { }
    public DbSet<Cell> Cells { get; set; }
    public DbSet<ReactorGrid> ReactorGrids { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var hasCellChanges = ChangeTracker.Entries<Cell>()
            .Any(e => e.State == EntityState.Added ||
                      e.State == EntityState.Deleted);

        if (hasCellChanges)
        {
            var trackedGrids = ChangeTracker.Entries<ReactorGrid>()
                .Select(e => e.Entity);

            foreach (var grid in trackedGrids)
            {
                grid.Invalidate();
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(ConfigurationManager.ConnectionString);
        }
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ReactorDbContext).Assembly,
            type => type.Namespace == "NuclearInfrastructure.Repositories.EntityConfigurations"
        );

        modelBuilder.Entity<Cell>(entity =>
        {
            entity.Property(c => c.Telemetry)
                  .HasColumnType("jsonb")
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<CellTelemetry>(v, (JsonSerializerOptions?)null) ?? new CellTelemetry()
                  );
        });

        modelBuilder.Entity<ReactorGrid>()
            .HasIndex(r => r.Name)
            .IsUnique();
    }
}

public class ReactorDbContextFactory : IDesignTimeDbContextFactory<ReactorDbContext>
{
    public ReactorDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ReactorDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("Default") ?? "Host=localhost;Database=testdb;User Id=postgres;Password=password");

        return new ReactorDbContext(optionsBuilder.Options);
    }
}