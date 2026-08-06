using Shared.Extensions;

namespace NuclearDomain.Entities.Telemetries;

public class ControlRodsTelemetryDto : CellTelemetry
{
    // 0.0 = Fully Extracted (No absorption)
    // 1.0 = Fully Inserted (Maximum absorption)
    public double TargetInsertionPercentage { get; set; } = 0.0;
    public double CurrentInsertionPercentage { get; set; } = 0.0;

    // Rod drive speed (e.g. moves 5% per second)
    public double DriveSpeedPercentPerSecond { get; set; } = 0.05;

    public void MoveTick(double deltaTimeSeconds)
    {
        if (Math.Abs(CurrentInsertionPercentage - TargetInsertionPercentage) > 0.001)
        {
            double step = DriveSpeedPercentPerSecond * deltaTimeSeconds;
            CurrentInsertionPercentage = MathExtensions.MoveTowards(
                CurrentInsertionPercentage,
                TargetInsertionPercentage,
                step
            );
        }
    }
}
