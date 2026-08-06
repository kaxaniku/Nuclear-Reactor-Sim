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
    public double NeutronFlux { get; set; }
    public double LocalPowerOutputMW { get; set; }
    public FuelRodStatus Status { get; set; }
    public bool IsOnline { get; set; } = false;

    public void ExecutePhysicsTick(double totalSuppression, double deltaTimeSeconds)
    {
        if (!IsOnline || Status == FuelRodStatus.Meltdown)
        {
            NeutronFlux = 0.0;
            LocalPowerOutputMW = 0.0;
            EvaluateStatus();
            return;
        }

        const double baseFlux = 100.0;
        const double fluxToPowerFactor = 0.05;
        const double powerToHeatFactor = 2.5;

        // Flux reduced by nearby control rod suppression
        NeutronFlux = baseFlux * (1.0 - Math.Clamp(totalSuppression, 0.0, 1.0));

        // Power & Heat
        LocalPowerOutputMW = NeutronFlux * fluxToPowerFactor;
        double deltaTemp = LocalPowerOutputMW * powerToHeatFactor * deltaTimeSeconds;
        TemperatureCelsius += deltaTemp;

        EvaluateStatus();
    }

    private void EvaluateStatus()
    {
        if (Status == FuelRodStatus.Scrammed && TemperatureCelsius < 600.0)
        {
            // Retain Scrammed state until cooled down
            return;
        }

        Status = TemperatureCelsius switch
        {
            >= 1200.0 => FuelRodStatus.Meltdown,
            >= 900.0 => FuelRodStatus.Critical,
            >= 600.0 => FuelRodStatus.Warning,
            _ => FuelRodStatus.Nominal
        };

        // Automatic failure state on Meltdown
        if (Status == FuelRodStatus.Meltdown)
        {
            IsOnline = false;
            NeutronFlux = 0.0;
            LocalPowerOutputMW = 0.0;
        }
    }
}