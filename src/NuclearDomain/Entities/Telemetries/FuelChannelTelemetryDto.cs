using NuclearDomain.Entities.Telemetries;

namespace NuclearDomain.Entities.Telemetries;

public enum FuelRodStatus
{
    Nominal,
    Warning,
    Scrammed,
    Critical,
    Meltdown
}

public class FuelChannelTelemetryDto : CellTelemetry
{
    public double LocalPowerOutputMW { get; set; }
    public FuelRodStatus Status { get; set; } = FuelRodStatus.Nominal;
    public bool IsOnline { get; set; } = true;

    /// <summary>
    /// Updates status and meltdown conditions based on temperature evaluated by the engine.
    /// </summary>
    public void ExecutePhysicsTick(double deltaTimeSeconds)
    {
        EvaluateStatus();

        if (Status == FuelRodStatus.Meltdown)
        {
            IsOnline = false;
            ThermalFlux = 0.0;
            FastFlux = 0.0;
            LocalPowerOutputMW = 0.0;
        }
    }

    private void EvaluateStatus()
    {
        if (Status == FuelRodStatus.Scrammed && TemperatureCelsius < 600.0)
        {
            return;
        }

        Status = TemperatureCelsius switch
        {
            >= 1200.0 => FuelRodStatus.Meltdown,
            >= 900.0 => FuelRodStatus.Critical,
            >= 600.0 => FuelRodStatus.Warning,
            _ => FuelRodStatus.Nominal
        };
    }
}