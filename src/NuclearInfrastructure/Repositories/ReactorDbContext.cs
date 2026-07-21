using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using NuclearApp.DTOs;

namespace NuclearInfrastructure.Repositories;

public class ReactorDbContext : DbContext
{
    public ReactorDbContext(DbContextOptions<ReactorDbContext> options) : base(options) { }
    public DbSet<ReactorOverviewDto> ReactorOverviews { get; set; }
    public DbSet<CellDto> Cells { get; set; }
    public DbSet<ConfigureCellCommandDto> ConfigureCellCommands { get; set; }
    public DbSet<MoveControlRodCommandDto> MoveControlRodCommands { get; set; }
    public DbSet<ReactorGridDto> ReactorGrids { get; set; }
    public DbSet<ScramReactorCommandDto> ScramReactorCommands { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(ConfigurationManager.ConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ReactorDbContext).Assembly,
            type => type.Namespace == "NuclearInfrastructure.Repositories.EntityConfigurations"
        );

        modelBuilder.Owned<CellTelemetryDto>();

        base.OnModelCreating(modelBuilder);
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