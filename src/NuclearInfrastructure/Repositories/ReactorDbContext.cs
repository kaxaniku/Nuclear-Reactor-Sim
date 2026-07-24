using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using NuclearDomain.DTOs;

namespace NuclearInfrastructure.Repositories;

public class ReactorDbContext : DbContext
{
    public ReactorDbContext(DbContextOptions<ReactorDbContext> options) : base(options) { }
    public DbSet<CellDto> Cells { get; set; }
    public DbSet<ReactorGridDto> ReactorGrids { get; set; }

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

        modelBuilder.Entity<CellDto>(entity =>
        {
            entity.Property(c => c.Telemetry)
                  .HasColumnType("jsonb")
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<CellTelemetryDto>(v, (JsonSerializerOptions?)null) ?? new CellTelemetryDto()
                  );
        });
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