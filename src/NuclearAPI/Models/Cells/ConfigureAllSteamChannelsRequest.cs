using NuclearDomain.Entities.Telemetries;

namespace Nuclear_Reactor_Sim.Models.Cells;

public class ConfigureAllSteamChannelsRequest
{
    public int ReactorGridId { get; set; }
    public SteamType Type { get; set; }
}
