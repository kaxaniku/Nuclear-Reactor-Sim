namespace NuclearApp.DTOs;

public record ReactorOverviewDto(
    int ReactorId,
    string Name,
    double AverageTemperature,
    double TotalPowerOutputMW,
    double AverageNeutronFlux,
    int ActiveFuelChannels,
    bool IsRunning
);
