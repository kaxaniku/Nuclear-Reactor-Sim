using MediatR;
using Microsoft.EntityFrameworkCore;
using Nuclear_Reactor_Sim.Extensions;
using Nuclear_Reactor_Sim.Middlewares;
using NuclearApp.Behaviors;
using NuclearApp.Interfaces.Repositories;
using NuclearInfrastructure.Repositories;
using Serilog;
using Serilog.Exceptions;

namespace Nuclear_Reactor_Sim;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.WithExceptionDetails()
            .CreateLogger();

        builder.Host.UseSerilog();
        builder.ConfigureAuth();
        builder.ConfigureBearer();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddDbContext<ReactorDbContext>(options =>
            options.UseNpgsql(
                ResolvePostgresConnectionString(builder.Configuration),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(ReactorDbContext).Assembly.FullName)
            )
            .UseSnakeCaseNamingConvention());

        //Add Service interfaces
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(NuclearApp.AssemblyReference).Assembly);

            cfg.AddOpenBehavior(typeof(ReactorValidationBehavior<,>));
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("OpenPolicy", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<ReactorDbContext>();

                var pendingMigrations = context.Database.GetPendingMigrations();
                if (pendingMigrations.Any())
                {
                    app.Logger.LogInformation("Applying {Count} pending migration(s)...", pendingMigrations.Count());
                    context.Database.Migrate();
                    app.Logger.LogInformation("Database migration completed successfully.");
                }
                else
                {
                    app.Logger.LogInformation("Database is already up to date.");
                }
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "An error occurred while migrating the database.");
                throw;
            }
        }

        app.UseSerilogRequestLogging();

        // Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment())
        //{
        //    app.UseSwagger();
        //    app.UseSwaggerUI();
        //}
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "RBMK API v1");

            options.RoutePrefix = string.Empty;
        });

        app.UseExceptionHandler();

        app.UseHttpsRedirection();

        app.UseCors("OpenPolicy");

        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }

    static string ResolvePostgresConnectionString(IConfiguration config)
    {
        var raw = Environment.GetEnvironmentVariable("DATABASE_URL")
                  ?? config.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException("No database connection configured (DATABASE_URL or ConnectionStrings:Default).");

        if (!raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            && !raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return raw;

        var uri = new Uri(raw);
        var userInfo = uri.UserInfo.Split(':', 2);
        var builder = new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
            Database = uri.AbsolutePath.TrimStart('/'),
            SslMode = Npgsql.SslMode.Prefer
        };
        return builder.ConnectionString;
    }
}