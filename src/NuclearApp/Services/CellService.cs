using NuclearApp.DTOs;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;

namespace NuclearApp.Services;

public class CellService
{
    private readonly IUnitOfWork _unitOfWork;

    public CellService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    // Move control rod in a cell
    public async Task MoveControlRodAsync(int reactorGridId, MoveControlRodCommandDto command, CancellationToken cancellationToken = default)
    {
        var reactorGrid = await _unitOfWork.ReactorGridRepository.GetByIdAsync(reactorGridId, cancellationToken);
        if (reactorGrid == null)
            throw new InvalidOperationException($"Reactor grid with ID {reactorGridId} not found.");

        var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == command.X && c.Y == command.Y);
        if (cell == null)
            throw new InvalidOperationException($"Cell at position ({command.X}, {command.Y}) not found in reactor grid.");

        if (cell.ColumnType != ColumnType.ControlRods)
            throw new InvalidOperationException("The specified cell does not contain control rods.");

        var telemetry = cell.Telemetry as ControlRodsTelemetryDto;
        if (telemetry == null)
            throw new InvalidOperationException("Invalid telemetry type for control rods.");

        if (command.TargetInsertionPercentage < 0 || command.TargetInsertionPercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(command.TargetInsertionPercentage), "Target insertion percentage must be between 0 and 100.");

        telemetry.InsertionLevel = command.TargetInsertionPercentage / 100.0;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // Set temperature for graphite moderator
    public async Task SetGraphiteModeratorTemperatureAsync(int reactorGridId, int x, int y, double temperatureCelsius, CancellationToken cancellationToken = default)
    {
        var reactorGrid = await _unitOfWork.ReactorGridRepository.GetByIdAsync(reactorGridId, cancellationToken);
        if (reactorGrid == null)
            throw new InvalidOperationException($"Reactor grid with ID {reactorGridId} not found.");

        var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == x && c.Y == y);
        if (cell == null)
            throw new InvalidOperationException($"Cell at position ({x}, {y}) not found in reactor grid.");

        if (cell.ColumnType != ColumnType.GraphiteModerator)
            throw new InvalidOperationException("The specified cell does not contain a graphite moderator.");

        var telemetry = cell.Telemetry as CellTelemetry;
        if (telemetry == null)
            throw new InvalidOperationException("Invalid telemetry type for graphite moderator.");

        telemetry.TemperatureCelsius = temperatureCelsius;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // Set absorption level for absorber
    public async Task SetAbsorberAbsorptionLevelAsync(int reactorGridId, int x, int y, double absorptionLevel, CancellationToken cancellationToken = default)
    {
        var reactorGrid = await _unitOfWork.ReactorGridRepository.GetByIdAsync(reactorGridId, cancellationToken);
        if (reactorGrid == null)
            throw new InvalidOperationException($"Reactor grid with ID {reactorGridId} not found.");

        var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == x && c.Y == y);
        if (cell == null)
            throw new InvalidOperationException($"Cell at position ({x}, {y}) not found in reactor grid.");

        if (cell.ColumnType != ColumnType.Absorber)
            throw new InvalidOperationException("The specified cell does not contain an absorber.");

        var telemetry = cell.Telemetry as AbsorberTelemetryDto;
        if (telemetry == null)
            throw new InvalidOperationException("Invalid telemetry type for absorber.");

        // Ensure the absorption level is within valid range
        if (absorptionLevel < 0 || absorptionLevel > 100)
            throw new ArgumentOutOfRangeException(nameof(absorptionLevel), "Absorption level must be between 0 and 100.");

        telemetry.AbsorptionLevel = absorptionLevel / 100.0;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // Set water flow rate and coolant level for cooler
    public async Task ConfigureCoolerAsync(int reactorGridId, int x, int y, double waterFlowRate, double coolantLevelPercent, CancellationToken cancellationToken = default)
    {
        var reactorGrid = await _unitOfWork.ReactorGridRepository.GetByIdAsync(reactorGridId, cancellationToken);
        if (reactorGrid == null)
            throw new InvalidOperationException($"Reactor grid with ID {reactorGridId} not found.");

        var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == x && c.Y == y);
        if (cell == null)
            throw new InvalidOperationException($"Cell at position ({x}, {y}) not found in reactor grid.");

        if (cell.ColumnType != ColumnType.Cooler)
            throw new InvalidOperationException("The specified cell does not contain a cooler.");

        var telemetry = cell.Telemetry as CoolerTelemetryDto;
        if (telemetry == null)
            throw new InvalidOperationException("Invalid telemetry type for cooler.");

        // Ensure the coolant level percent is within valid range
        if (coolantLevelPercent < 0 || coolantLevelPercent > 100)
            throw new ArgumentOutOfRangeException(nameof(coolantLevelPercent), "Coolant level percentage must be between 0 and 100.");

        telemetry.WaterFlowRate = waterFlowRate;
        telemetry.CoolantLevelPercent = coolantLevelPercent / 100.0;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // Configure steam channel parameters
    public async Task ConfigureSteamChannelAsync(int reactorGridId, int x, int y, double steamGenerationRate, double pressure, double quality, SteamType type, CancellationToken cancellationToken = default)
    {
        var reactorGrid = await _unitOfWork.ReactorGridRepository.GetByIdAsync(reactorGridId, cancellationToken);
        if (reactorGrid == null)
            throw new InvalidOperationException($"Reactor grid with ID {reactorGridId} not found.");

        var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == x && c.Y == y);
        if (cell == null)
            throw new InvalidOperationException($"Cell at position ({x}, {y}) not found in reactor grid.");

        if (cell.ColumnType != ColumnType.SteamChannel)
            throw new InvalidOperationException("The specified cell does not contain a steam channel.");

        var telemetry = cell.Telemetry as SteamChannelTelemetryDto;
        if (telemetry == null)
            throw new InvalidOperationException("Invalid telemetry type for steam channel.");

        telemetry.SteamGenerationRateMW = steamGenerationRate;
        telemetry.PressureBar = pressure;
        telemetry.SteamQuality = quality;
        telemetry.SteamType = type;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // Configure fuel channel parameters
    public async Task ConfigureFuelChannelAsync(int reactorGridId, int x, int y, double neutronFlux, double localPowerOutput, string status, CancellationToken cancellationToken = default)
    {
        var reactorGrid = await _unitOfWork.ReactorGridRepository.GetByIdAsync(reactorGridId, cancellationToken);
        if (reactorGrid == null)
            throw new InvalidOperationException($"Reactor grid with ID {reactorGridId} not found.");

        var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == x && c.Y == y);
        if (cell == null)
            throw new InvalidOperationException($"Cell at position ({x}, {y}) not found in reactor grid.");

        if (cell.ColumnType != ColumnType.FuelChannel)
            throw new InvalidOperationException("The specified cell does not contain a fuel channel.");

        var telemetry = cell.Telemetry as FuelChannelTelemetryDto;
        if (telemetry == null)
            throw new InvalidOperationException("Invalid telemetry type for fuel channel.");

        telemetry.NeutronFlux = neutronFlux;
        telemetry.LocalPowerOutputMW = localPowerOutput;
        telemetry.Status = status;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}