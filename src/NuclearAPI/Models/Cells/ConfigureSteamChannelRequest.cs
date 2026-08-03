using NuclearDomain.Entities;

namespace Nuclear_Reactor_Sim.Models.Cells;

public class ConfigureSteamChannelRequest
{
    public int ReactorGridId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public double SteamGenerationRateMW { get; set; }
    public double PressureBar { get; set; }
    public double Quality { get; set; }
    public SteamType Type { get; set; }
}
