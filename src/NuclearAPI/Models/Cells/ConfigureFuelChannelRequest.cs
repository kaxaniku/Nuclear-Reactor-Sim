using NuclearDomain.Entities.Telemetries;

namespace Nuclear_Reactor_Sim.Models.Cells;

public class ConfigureFuelChannelRequest
{
    public int ReactorGridId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public FuelRodStatus Status { get; set; }
}