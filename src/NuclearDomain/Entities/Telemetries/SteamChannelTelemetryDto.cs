namespace NuclearDomain.Entities.Telemetries;

public enum SteamType
{
    Normal,         // Low pressure / subcooled boiling (~100°C saturation)
    Dense,          // High-pressure saturated wet steam (~70 bar, ~285.8°C saturation)
    Superheated,    // Dry steam heated beyond boiling temperature
    Supercritical   // Above critical point (>221.2 bar, >374°C - liquid/gas phase boundary disappears)
}

public class SteamChannelTelemetryDto : CellTelemetry
{
    /// <summary>
    /// Target pressure maintained by the main steam drum separator and turbine bypass valves (Bar).
    /// Range: 1.0 bar (Atmospheric) to 100.0+ bar (Overpressurized). Default: 70.0 bar (RBMK operational).
    /// </summary>
    public double TargetPressureBar { get; set; } = 70.0;
    public double InletWaterTemperatureCelsius { get; set; } = 20.0;

    /// <summary>
    /// Main Circulation Pump (MCP) coolant intake throttling (0.0 = Valve Closed/No Flow, 1.0 = Max Flow).
    /// Lower flow causes water to dwell longer and superheat; higher flow keeps it subcooled/dense.
    /// </summary>
    public double FlowRateThrottling { get; set; } = 0.3;
    public double SteamGenerationRateMW { get; set; }
    public double PressureBar { get; set; } = 70.0; // RBMK-1000 standard operational pressure
    public double SteamQuality { get; set; }        // Void fraction (0.0 = all liquid water, 1.0 = 100% steam)
    public SteamType SteamType { get; private set; } = SteamType.Dense;

    // Saturation temperature (boiling point) scales directly with pressure
    public double TargetPhaseTemperatureCelsius => PressureBar switch
    {
        <= 1.0 => 100.0,
        >= 221.2 => 374.0, // Critical point of water
        >= 70.0 => 285.8,  // RBMK-1000 operational saturation point
        _ => 100.0 + (PressureBar * 2.65) // Linear approximation for intermediate pressures
    };

    // Latent heat of vaporization decreases as pressure compresses steam density
    private double LatentHeatVaporization => PressureBar switch
    {
        <= 1.0 => 2.26,   // MJ/kg at atmospheric pressure
        >= 221.2 => 0.0,  // Latent heat reaches zero at critical point
        >= 70.0 => 1.50,  // MJ/kg at 70 bar
        _ => 2.26 - (PressureBar * 0.0108)
    };

    private const double WaterSpecificHeat = 0.004184; // MJ / (kg * °C) (Liquid water)
    private const double SteamSpecificHeat = 0.002100; // MJ / (kg * °C) (Superheated steam gas)

    public void ProcessCoolingAndSteam(double thermalEnergyInputMJ, double baseWaterMassKg, double deltaTimeSeconds)
    {
        // 1. Smoothly adjust current pressure toward target
        PressureBar += (TargetPressureBar - PressureBar) * Math.Min(1.0, deltaTimeSeconds * 0.5);

        double targetSatTemp = TargetPhaseTemperatureCelsius;

        // Mass flow through channel in Kg/sec driven by MCP throttling
        // High flow replaces channel volume multiple times per second
        double massFlowRateKgPerSec = baseWaterMassKg * FlowRateThrottling * 0.5;
        double incomingColdWaterMassKg = massFlowRateKgPerSec * deltaTimeSeconds;

        // 2. Cold Water Inflow Flushing (Mass Balance)
        // Continuous cold water inflow dilutes existing steam quality and cools channel pool
        if (incomingColdWaterMassKg > 0.0)
        {
            double replacementFraction = Math.Min(1.0, incomingColdWaterMassKg / baseWaterMassKg);

            // Quality flushed out by fresh water intake
            SteamQuality = Math.Max(0.0, SteamQuality * (1.0 - replacementFraction));

            double convectiveCoolingRate = 0.15 * FlowRateThrottling * deltaTimeSeconds;
            TemperatureCelsius += (InletWaterTemperatureCelsius - TemperatureCelsius) * convectiveCoolingRate;
        }

        if (thermalEnergyInputMJ <= 0.0)
        {
            SteamGenerationRateMW = 0.0;
            UpdateSteamType();
            return;
        }

        // 3. Sensible Heating (Subcooled Liquid -> Saturation Temp)
        if (TemperatureCelsius < targetSatTemp)
        {
            SteamQuality = 0.0;

            double energyNeededToBoil = baseWaterMassKg * WaterSpecificHeat * (targetSatTemp - TemperatureCelsius);

            if (thermalEnergyInputMJ <= energyNeededToBoil)
            {
                TemperatureCelsius += thermalEnergyInputMJ / (baseWaterMassKg * WaterSpecificHeat);
                SteamGenerationRateMW = 0.0;
                UpdateSteamType();
                return;
            }

            TemperatureCelsius = targetSatTemp;
            thermalEnergyInputMJ -= energyNeededToBoil;
        }

        // 4. Latent Heat Phase Change (Boiling Water -> Wet Steam)
        if (SteamQuality < 1.0 && PressureBar < 221.2)
        {
            double latentHeat = LatentHeatVaporization;
            double steamProducedKg = thermalEnergyInputMJ / latentHeat;

            SteamGenerationRateMW = thermalEnergyInputMJ / deltaTimeSeconds;
            double qualityDelta = steamProducedKg / baseWaterMassKg;

            if (SteamQuality + qualityDelta <= 1.0)
            {
                SteamQuality += qualityDelta;
                UpdateSteamType();
                return;
            }

            // Channel completely dried out this frame; remaining energy superheats steam
            double unusedEnergyMJ = (SteamQuality + qualityDelta - 1.0) * baseWaterMassKg * latentHeat;
            SteamQuality = 1.0;
            thermalEnergyInputMJ = unusedEnergyMJ;
        }

        // 5. Superheating Phase (Dry Steam Gas Heating)
        TemperatureCelsius += thermalEnergyInputMJ / (baseWaterMassKg * SteamSpecificHeat);
        SteamGenerationRateMW = thermalEnergyInputMJ / deltaTimeSeconds;

        UpdateSteamType();
    }

    private void UpdateSteamType()
    {
        if (PressureBar >= 221.2)
        {
            SteamType = SteamType.Supercritical;
        }
        else if (SteamQuality >= 1.0)
        {
            SteamType = SteamType.Superheated;
        }
        else if (PressureBar >= 30.0)
        {
            SteamType = SteamType.Dense;
        }
        else
        {
            SteamType = SteamType.Normal;
        }
    }
}