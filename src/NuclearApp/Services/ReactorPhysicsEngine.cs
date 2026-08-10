using NuclearApp.Interfaces.Services;
using NuclearDomain.Entities;
using NuclearDomain.Entities.Telemetries;

namespace NuclearApp.Services;

public class ReactorPhysicsEngine : IReactorPhysicsEngine
{
    private const double InfluenceRadius = 2.5;
    private const double FuelMassKg = 115.0;
    private const double GraphiteMassKg = 1400.0;
    private const double FuelCp = 0.00030;      // MJ / (kg * °C)
    private const double GraphiteCp = 0.00180;  // MJ / (kg * °C)

    // Core Feedback Coefficients
    private const double BaseKInfinitive = 1.05;        // Fresh 2.0% enriched UO2
    private const double DopplerCoefficient = -0.00018; // -Δk per °C fuel rise
    private const double VoidCoefficient = +0.00015;    // +Δk per % void

    // INCREASED: Allows realistic MW power generation from Thermal Flux
    private const double FluxToMWFactor = 0.5;

    private static readonly (int dx, int dy)[] Directions = new[]
    {
        (0, 1),   // Up
        (0, -1),  // Down
        (-1, 0),  // Left
        (1, 0)    // Right
    };

    public void ProcessPhysicsTick(ReactorGrid grid, double deltaTimeSeconds)
    {
        if (!grid.IsRunning || !grid.IsValid)
        {
            return;
        }

        var cellMap = grid.Cells.ToDictionary(c => (c.X, c.Y));

        // 1. Advance Mechanical Devices
        var controlRods = ControlRodsPhysics(grid, deltaTimeSeconds);

        // 2. Neutron Diffusion & Chain Reaction Pass
        ProcessDiffusionAndNeutronChain(cellMap, controlRods, deltaTimeSeconds);

        // 3. Fuel Fission Power & Single-Source Thermal Heating
        FuelChannelPhysics(cellMap, deltaTimeSeconds, controlRods);

        // 4. Spatial Conduction & Thermal-Hydraulic Phase Change
        ProcessConductionAndPhaseChange(cellMap, deltaTimeSeconds);

        foreach (var cell in grid.Cells)
        {
            if (cell.Telemetry is FuelChannelTelemetryDto fuel)
            {
                fuel.ExecutePhysicsTick(deltaTimeSeconds);
            }
        }
    }

    private static List<Cell> ControlRodsPhysics(ReactorGrid grid, double deltaTimeSeconds)
    {
        var controlRods = grid.Cells
            .Where(c => c.ColumnType == ColumnType.ControlRods)
            .ToList();

        foreach (var rodCell in controlRods)
        {
            if (rodCell.Telemetry is ControlRodsTelemetryDto rodTelemetry)
            {
                rodTelemetry.MoveTick(deltaTimeSeconds);
            }
        }

        return controlRods;
    }

    private static void ProcessDiffusionAndNeutronChain(
        Dictionary<(int X, int Y), Cell> cellMap,
        List<Cell> controlRods,
        double deltaTimeSeconds)
    {
        var newFastFlux = new Dictionary<(int X, int Y), double>();
        var newThermalFlux = new Dictionary<(int X, int Y), double>();

        // Pass 1: Retain un-diffused flux and handle spatial diffusion across neighbors
        foreach (var ((x, y), cell) in cellMap)
        {
            if (cell.Telemetry is not CellTelemetry telemetry) continue;

            // Leak rate scaled by time (10% fast, 8% thermal lost per second)
            double fastLeak = telemetry.FastFlux * Math.Min(1.0, 0.10 * deltaTimeSeconds);
            double thermalLeak = telemetry.ThermalFlux * Math.Min(1.0, 0.08 * deltaTimeSeconds);

            // Keep local un-diffused portion
            AddDelta(newFastFlux, (x, y), telemetry.FastFlux - fastLeak);
            AddDelta(newThermalFlux, (x, y), telemetry.ThermalFlux - thermalLeak);

            double outgoingFast = fastLeak / 4.0;
            double outgoingThermal = thermalLeak / 4.0;

            foreach (var (dx, dy) in Directions)
            {
                var neighborPos = (x + dx, y + dy);
                if (!cellMap.TryGetValue(neighborPos, out var neighborCell))
                    continue;

                switch (neighborCell.ColumnType)
                {
                    case ColumnType.GraphiteModerator:
                        // Graphite moderates fast neutrons into thermal neutrons
                        AddDelta(newThermalFlux, neighborPos, outgoingFast * 0.95);
                        break;

                    case ColumnType.Reflector:
                        // Reflector bounces fast flux back into source cell
                        AddDelta(newFastFlux, (x, y), outgoingFast * 0.85);
                        AddDelta(newThermalFlux, (x, y), outgoingThermal * 0.85);
                        AddDelta(newThermalFlux, (x, y), outgoingFast * 0.10);
                        break;

                    case ColumnType.ControlRods:
                        if (neighborCell.Telemetry is ControlRodsTelemetryDto rod)
                        {
                            double passedThermal = outgoingThermal * (1.0 - rod.CurrentInsertionPercentage);
                            AddDelta(newThermalFlux, neighborPos, passedThermal);
                        }
                        break;

                    default:
                        AddDelta(newFastFlux, neighborPos, outgoingFast * 0.80);
                        AddDelta(newThermalFlux, neighborPos, outgoingThermal * 0.80);
                        break;

                }
            }
        }

        // Pass 2: Fission Chain Reaction in Fuel Channels
        foreach (var ((x, y), cell) in cellMap)
        {
            if (cell.ColumnType == ColumnType.FuelChannel && cell.Telemetry is FuelChannelTelemetryDto fuel)
            {
                if (!fuel.IsOnline || fuel.Status == FuelRodStatus.Meltdown)
                    continue;

                double nearbyVoidFraction = GetAverageNearbySteamQuality(cellMap, x, y);

                double deltaKDoppler = (fuel.TemperatureCelsius - 20.0) * DopplerCoefficient;
                double deltaKVoid = nearbyVoidFraction * VoidCoefficient * 100.0;
                double localK = BaseKInfinitive + deltaKDoppler + deltaKVoid;

                // Use newly calculated thermal flux from Pass 1 (moderated from graphite)
                double currentThermalInCell = newThermalFlux.GetValueOrDefault((x, y), fuel.ThermalFlux);

                // Ensure a minimal seed only when thermal flux is critically low during startup
                double effectiveThermal = Math.Max(currentThermalInCell, 20.0);

                // Thermal neutrons absorbed to induce fission
                double thermalAbsorbed = effectiveThermal * (0.25 * deltaTimeSeconds);

                // Generate fast neutrons from fission
                double fissionFastFlux = thermalAbsorbed * localK * 2.0; // Scaled so production > leakage when localK > 1.0

                // Subtract consumed thermal flux and add generated fast flux
                AddDelta(newThermalFlux, (x, y), -thermalAbsorbed);
                AddDelta(newFastFlux, (x, y), fissionFastFlux);
            }
        }

        // Apply updated flux levels to telemetry state
        foreach (var ((x, y), cell) in cellMap)
        {
            if (cell.Telemetry is CellTelemetry t)
            {
                t.FastFlux = Math.Max(0.0, newFastFlux.GetValueOrDefault((x, y), 0.0));
                t.ThermalFlux = Math.Max(0.0, newThermalFlux.GetValueOrDefault((x, y), 0.0));
            }
        }
    }

    private static void FuelChannelPhysics(
        Dictionary<(int X, int Y), Cell> cellMap,
        double deltaTimeSeconds,
        List<Cell> controlRods)
    {
        foreach (var ((x, y), cell) in cellMap)
        {
            if (cell.ColumnType != ColumnType.FuelChannel || cell.Telemetry is not FuelChannelTelemetryDto fuelTelemetry)
                continue;

            if (!fuelTelemetry.IsOnline || fuelTelemetry.Status == FuelRodStatus.Meltdown)
            {
                fuelTelemetry.LocalPowerOutputMW = 0.0;
                fuelTelemetry.ExecutePhysicsTick(deltaTimeSeconds);
                continue;
            }

            // Calculate total control rod suppression within InfluenceRadius
            double totalSuppression = 0.0;
            foreach (var rodCell in controlRods)
            {
                if (rodCell.Telemetry is ControlRodsTelemetryDto rodTelemetry)
                {
                    double dx = cell.X - rodCell.X;
                    double dy = cell.Y - rodCell.Y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    if (distance <= InfluenceRadius)
                    {
                        double proximityFactor = 1.0 - (distance / InfluenceRadius);
                        totalSuppression += rodTelemetry.CurrentInsertionPercentage * proximityFactor;
                    }
                }
            }

            // Suppress effective thermal flux driving fission power
            double effectiveThermalFlux = Math.Max(0.0, fuelTelemetry.ThermalFlux * (1.0 - Math.Clamp(totalSuppression, 0.0, 1.0)));
            fuelTelemetry.LocalPowerOutputMW = effectiveThermalFlux * FluxToMWFactor;

            // Thermal energy generated (MJ = MW * seconds)
            double thermalEnergyGeneratedMJ = fuelTelemetry.LocalPowerOutputMW * deltaTimeSeconds;

            // Single source of heating truth
            double deltaT = thermalEnergyGeneratedMJ / (FuelMassKg * FuelCp);
            fuelTelemetry.TemperatureCelsius += deltaT;
        }
    }

    private static void ProcessConductionAndPhaseChange(
    Dictionary<(int X, int Y), Cell> cellMap,
    double deltaTimeSeconds)
    {
        const double ThermalConductivity = 0.001;
        var energyTransfersMJ = new Dictionary<(int X, int Y), double>();

        // 1. Calculate Conduction Transfers
        foreach (var ((x, y), cell) in cellMap)
        {
            if (cell.Telemetry is not CellTelemetry source) continue;

            double sourceMass = GetCellMass(cell.ColumnType);
            double sourceCp = GetCellCp(cell.ColumnType);
            double energyToEqualizeMJ = sourceMass * sourceCp;

            foreach (var (dx, dy) in Directions)
            {
                var neighborPos = (x + dx, y + dy);
                if (cellMap.TryGetValue(neighborPos, out var neighbor) && neighbor.Telemetry is CellTelemetry target)
                {
                    double tempDiff = source.TemperatureCelsius - target.TemperatureCelsius;

                    if (tempDiff > 0.0)
                    {
                        double energyTransferredMJ = tempDiff * ThermalConductivity * deltaTimeSeconds;

                        // Limit transfer per neighbor so 4 neighbors combined can't drain >32% of heat per tick
                        double maxTransferMJ = (tempDiff * 0.08) * energyToEqualizeMJ;
                        energyTransferredMJ = Math.Min(energyTransferredMJ, maxTransferMJ);

                        AddDelta(energyTransfersMJ, (x, y), -energyTransferredMJ);
                        AddDelta(energyTransfersMJ, neighborPos, energyTransferredMJ);
                    }
                }
            }
        }

        // 2. Apply ONLY Conduction Deltas & Execute Phase Change
        foreach (var ((x, y), cell) in cellMap)
        {
            if (cell.Telemetry is not CellTelemetry telemetry) continue;

            double netConductionEnergyMJ = energyTransfersMJ.GetValueOrDefault((x, y), 0.0);

            if (cell.ColumnType == ColumnType.SteamChannel && cell.Telemetry is SteamChannelTelemetryDto steam)
            {
                steam.ProcessCoolingAndSteam(netConductionEnergyMJ, 250.0, deltaTimeSeconds);
            }
            else
            {
                double mass = GetCellMass(cell.ColumnType);
                double cp = GetCellCp(cell.ColumnType);

                // Apply ONLY the conduction gain/loss to the temperature already established by fission physics
                double deltaT = netConductionEnergyMJ / (mass * cp);
                telemetry.TemperatureCelsius = Math.Max(20.0, telemetry.TemperatureCelsius + deltaT);
            }
        }
    }

    private static double GetCellMass(ColumnType type) => type switch
    {
        ColumnType.GraphiteModerator => GraphiteMassKg, // 1400.0 kg
        ColumnType.FuelChannel => FuelMassKg,             // 115.0 kg
        _ => 200.0                                       // Control Rods, Absorbers, Reflectors
    };

    private static double GetCellCp(ColumnType type) => type switch
    {
        ColumnType.GraphiteModerator => GraphiteCp,     // 0.00071 MJ/(kg*°C)
        ColumnType.FuelChannel => FuelCp,                 // 0.00030 MJ/(kg*°C)
        _ => 0.00050                                     // Steel/B4C alloy estimate
    };

    private static double GetAverageNearbySteamQuality(Dictionary<(int X, int Y), Cell> cellMap, int x, int y)
    {
        double totalQuality = 0.0;
        int count = 0;

        foreach (var (dx, dy) in Directions)
        {
            if (cellMap.TryGetValue((x + dx, y + dy), out var neighbor) &&
                neighbor.ColumnType == ColumnType.SteamChannel &&
                neighbor.Telemetry is SteamChannelTelemetryDto steam)
            {
                totalQuality += steam.SteamQuality;
                count++;
            }
        }

        return count > 0 ? totalQuality / count : 0.0;
    }

    private static void AddDelta(Dictionary<(int X, int Y), double> dict, (int X, int Y) key, double value)
    {
        dict[key] = dict.GetValueOrDefault(key, 0.0) + value;
    }
}