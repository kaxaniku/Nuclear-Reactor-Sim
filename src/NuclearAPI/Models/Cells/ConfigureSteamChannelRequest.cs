using NuclearDomain.Entities.Telemetries;

namespace Nuclear_Reactor_Sim.Models.Cells;

public class ConfigureSteamChannelRequest
{
    public int ReactorGridId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public SteamType Type { get; set; }
    public double FlowRateThrottling { get; set; }
}
