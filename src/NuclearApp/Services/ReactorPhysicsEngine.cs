using System;
using System.Collections.Generic;
using System.Text;
using NuclearApp.Interfaces.Services;
using NuclearDomain.Entities;
using NuclearDomain.Entities.Telemetries;

namespace NuclearApp.Services;

public class ReactorPhysicsEngine : IReactorPhysicsEngine
{
    private const double InfluenceRadius = 2.5;

    public void ProcessPhysicsTick(ReactorGrid grid, double deltaTimeSeconds)
    {
        if (!grid.IsRunning || !grid.IsValid)
        {
            return;
        }

        var controlRods = ControlRodsPhysics(grid, deltaTimeSeconds);

        var fuelChannels = FuelChannelPhysics(grid, deltaTimeSeconds, controlRods);
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

    private static List<Cell> FuelChannelPhysics(ReactorGrid grid, double deltaTimeSeconds, List<Cell> controlRods)
    {
        var fuelChannels = grid.Cells
            .Where(c => c.ColumnType == ColumnType.FuelChannel)
            .ToList();

        foreach (var fuelCell in fuelChannels)
        {
            if (fuelCell.Telemetry is FuelChannelTelemetryDto fuelTelemetry)
            {
                double totalSuppression = 0.0;

                foreach (var rodCell in controlRods)
                {
                    if (rodCell.Telemetry is ControlRodsTelemetryDto rodTelemetry)
                    {
                        double dx = fuelCell.X - rodCell.X;
                        double dy = fuelCell.Y - rodCell.Y;
                        double distance = Math.Sqrt(dx * dx + dy * dy);

                        if (distance <= InfluenceRadius)
                        {
                            double proximityFactor = 1.0 - (distance / InfluenceRadius);
                            totalSuppression += rodTelemetry.CurrentInsertionPercentage * proximityFactor;
                        }
                    }
                }

                fuelTelemetry.ExecutePhysicsTick(totalSuppression, deltaTimeSeconds);
            }
        }

        return fuelChannels;
    }
}