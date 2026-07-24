using NuclearApp.Interfaces.Repositories;
using NuclearDomain.DTOs;

namespace NuclearInfrastructure.Repositories;

internal class ConfigureCellCommandRepository : BaseRepository<ConfigureCellCommandDto>, IConfigureCellCommandRepository
{
    public ConfigureCellCommandRepository(ReactorDbContext context) : base(context) { }
}