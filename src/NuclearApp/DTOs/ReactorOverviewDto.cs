namespace NuclearApp.DTOs;

public record ReactorOverviewDto(
    int ReactorId,
    string Name,
    double AverageTemperature,
    double TotalPowerOutputMW,
    double TotalSteamGenerationMW,
    double AverageNeutronFlux,
    int ActiveFuelChannels,
    int TotalFuelChannels,
    bool IsRunning
);