using NuclearApp.Interfaces.Repositories;
using NuclearDomain.DTOs;

namespace NuclearInfrastructure.Repositories;

internal class MoveControlRodCommandRepository : BaseRepository<MoveControlRodCommandDto>, IMoveControlRodCommandRepository
{
    public MoveControlRodCommandRepository(ReactorDbContext context) : base(context) { }
}