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
    private const double DopplerCoefficient = -0.00027; // -Δk per °C fuel rise
    private const double VoidCoefficient = +0.00015;    // +Δk per % void

    // INCREASED: Allows realistic MW power generation from Thermal Flux
    private const double FluxToMWFactor = 1.2;

    private static readonly (int dx, int dy)[] Directions = new[]
    {
        (0, 1),   // Up
        (0, -1),  // Down
        (-1, 0),  // Left
        (1, 0)    // Right
    };

    public void ProcessPhysicsTick(ReactorGrid grid, double deltaTimeSeconds)
    {
        if (!grid.IsMonitored || !grid.IsValid)
        {
            return;
        }

        var cellMap = grid.Cells.ToDictionary(c => (c.X, c.Y));

        // 1. Advance Mechanical Devices
        var controlRods = ControlRodsPhysics(grid, deltaTimeSeconds);

        // 2. Neutron Diffusion & Chain Reaction Pass
        ProcessDiffusionAndNeutronChain(cellMap, controlRods, deltaTimeSeconds);

        // 3. Fuel Fission Power & Single-Source Thermal Heating
        FuelChannelPhysics(cellMap, deltaTimeSeconds);

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

    private static void ProcessDiffusionAndNeutronChain(Dictionary<(int X, int Y), Cell> cellMap, List<Cell> controlRods, double deltaTimeSeconds)
    {
        var newFastFlux = new Dictionary<(int X, int Y), double>();
        var newThermalFlux = new Dictionary<(int X, int Y), double>();

        // Continuous spontaneous source floor per cell (e.g., U-238 spontaneous fission / photoneutrons)
        const double SpontaneousSourceFloor = 0.5;

        // Pass 1: Spatial Diffusion & Moderation/Absorption
        foreach (var ((x, y), cell) in cellMap)
        {
            if (cell.Telemetry is not CellTelemetry telemetry) continue;

            // Diffusion rate: higher fraction migrates each second
            double fastDiffusionFraction = Math.Min(0.80, 0.40 * deltaTimeSeconds);
            double thermalDiffusionFraction = Math.Min(0.80, 0.30 * deltaTimeSeconds);

            double fastLeaving = telemetry.FastFlux * fastDiffusionFraction;
            double thermalLeaving = telemetry.ThermalFlux * thermalDiffusionFraction;

            // Retain un-diffused flux in the current cell
            AddDelta(newFastFlux, (x, y), telemetry.FastFlux - fastLeaving);
            AddDelta(newThermalFlux, (x, y), telemetry.ThermalFlux - thermalLeaving);

            double outgoingFastPerNeighbor = fastLeaving / 4.0;
            double outgoingThermalPerNeighbor = thermalLeaving / 4.0;

            foreach (var (dx, dy) in Directions)
            {
                var neighborPos = (x + dx, y + dy);
                if (!cellMap.TryGetValue(neighborPos, out var neighborCell))
                    continue; // Boundary loss (outer leakage)

                switch (neighborCell.ColumnType)
                {
                    case ColumnType.GraphiteModerator:
                        // Graphite moderates fast neutrons into thermal neutrons efficiently
                        AddDelta(newThermalFlux, neighborPos, outgoingFastPerNeighbor * 0.98);
                        AddDelta(newThermalFlux, neighborPos, outgoingThermalPerNeighbor * 0.98);
                        break;

                    case ColumnType.FuelChannel:
                        // Fuel accepts fast and thermal flux directly
                        AddDelta(newFastFlux, neighborPos, outgoingFastPerNeighbor * 0.95);
                        AddDelta(newThermalFlux, neighborPos, outgoingThermalPerNeighbor * 0.95);
                        break;

                    case ColumnType.Reflector:
                        // Bounces flux back to the source cell (reflection)
                        AddDelta(newFastFlux, (x, y), outgoingFastPerNeighbor * 0.90);
                        AddDelta(newThermalFlux, (x, y), outgoingThermalPerNeighbor * 0.90);
                        break;

                    case ColumnType.ControlRods:
                        if (neighborCell.Telemetry is ControlRodsTelemetryDto rod)
                        {
                            double absorption = Math.Clamp(rod.CurrentInsertionPercentage / 100.0, 0.0, 1.0);
                            double passedThermal = outgoingThermalPerNeighbor * (1.0 - absorption);
                            double passedFast = outgoingFastPerNeighbor * (1.0 - (absorption * 0.85));

                            AddDelta(newFastFlux, neighborPos, passedFast);
                            AddDelta(newThermalFlux, neighborPos, passedThermal);
                        }
                        break;

                    default:
                        AddDelta(newFastFlux, neighborPos, outgoingFastPerNeighbor * 0.90);
                        AddDelta(newThermalFlux, neighborPos, outgoingThermalPerNeighbor * 0.90);
                        break;
                }
            }
        }

        // Pass 1.5: Control Rod Internal Sink Destruction
        foreach (var rodCell in controlRods)
        {
            if (rodCell.Telemetry is ControlRodsTelemetryDto rod)
            {
                double absorption = Math.Clamp(rod.CurrentInsertionPercentage / 100.0, 0.0, 1.0);
                var pos = (rodCell.X, rodCell.Y);

                if (newThermalFlux.TryGetValue(pos, out double currentThermal))
                    newThermalFlux[pos] = currentThermal * (1.0 - absorption);
                if (newFastFlux.TryGetValue(pos, out double currentFast))
                    newFastFlux[pos] = currentFast * (1.0 - (absorption * 0.85));
            }
        }

        // Pass 2: Fission Chain Reaction & Moderation in Fuel Channels
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

                // 1. Fetch current pools
                double fastPool = newFastFlux.GetValueOrDefault((x, y), 0.0);
                double thermalPool = newThermalFlux.GetValueOrDefault((x, y), 0.0);

                // 2. Local Moderation (Fast -> Thermal)
                double FastToThermalRate = Math.Min(1.0, 0.50 * deltaTimeSeconds);
                double moderatedFast = fastPool * FastToThermalRate;

                fastPool -= moderatedFast;
                thermalPool += moderatedFast;

                // 3. Spontaneous background source floor
                thermalPool = Math.Max(thermalPool, SpontaneousSourceFloor);

                // 4. Fission (Thermal absorbed -> Fast generated)
                double absorptionRate = Math.Min(1.0, 0.60 * deltaTimeSeconds);
                double thermalAbsorbed = thermalPool * absorptionRate;

                // Fission generates NEW fast neutrons based on localK
                double fissionFastGenerated = thermalAbsorbed * localK * 2;

                thermalPool -= thermalAbsorbed;
                fastPool += fissionFastGenerated;

                // 5. Write back exact updated values
                newFastFlux[(x, y)] = fastPool;
                newThermalFlux[(x, y)] = thermalPool;
            }
        }

        // Synchronize calculated flux pools back to cell telemetry
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
    double deltaTimeSeconds)
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

            // Direct, unified single-source of power generation:
            fuelTelemetry.LocalPowerOutputMW = fuelTelemetry.ThermalFlux * FluxToMWFactor;

            // Thermal energy generated (MJ = MW * seconds)
            double thermalEnergyGeneratedMJ = fuelTelemetry.LocalPowerOutputMW * deltaTimeSeconds;

            // Apply temperature rise
            double deltaT = thermalEnergyGeneratedMJ / (FuelMassKg * FuelCp);
            fuelTelemetry.TemperatureCelsius += deltaT;
        }
    }

    private static void ProcessConductionAndPhaseChange(
    Dictionary<(int X, int Y), Cell> cellMap,
    double deltaTimeSeconds)
    {
        const double ThermalConductivity = 0.03;
        const double AmbientVaultTempCelsius = 20.0;
        const double AmbientCoolingCoeff = 0.0001;
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

                        double maxTransferMJ = (tempDiff * 0.1) * energyToEqualizeMJ;
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

                // 1. Apply conduction energy exchange with adjacent cells
                double deltaT = netConductionEnergyMJ / (mass * cp);
                telemetry.TemperatureCelsius += deltaT;
            }

            // 2. Apply passive ambient dissipation to ALL cells (Vault cooling)
            double tempDeltaToVault = telemetry.TemperatureCelsius - AmbientVaultTempCelsius;
            if (tempDeltaToVault > 0.0)
            {
                telemetry.TemperatureCelsius -= tempDeltaToVault * AmbientCoolingCoeff * deltaTimeSeconds;
            }

            // Clamp floor at ambient baseline
            telemetry.TemperatureCelsius = Math.Max(AmbientVaultTempCelsius, telemetry.TemperatureCelsius);
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