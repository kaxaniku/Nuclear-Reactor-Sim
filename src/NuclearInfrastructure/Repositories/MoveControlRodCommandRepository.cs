using NuclearApp.Interfaces.Repositories;
using NuclearApp.DTOs;

namespace NuclearInfrastructure.Repositories;

internal class MoveControlRodCommandRepository : BaseRepository<MoveControlRodCommandDto>, IMoveControlRodCommandRepository
{
    public MoveControlRodCommandRepository(ReactorDbContext context) : base(context) { }
}