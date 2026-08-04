using Microsoft.EntityFrameworkCore;
using NuclearApp.Interfaces.Repositories;
using NuclearEngine.Workers;
using NuclearInfrastructure.Repositories;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<ReactorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
           .UseSnakeCaseNamingConvention());

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(NuclearApp.AssemblyReference).Assembly));

builder.Services.AddHostedService<ReactorSimulationWorker>();


var host = builder.Build();
host.Run();
