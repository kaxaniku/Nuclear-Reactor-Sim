using Moq;
using NuclearApp.Interfaces.Repositories;

namespace NuclearUnitTests;

public abstract class CommandHandlerBaseTests
{
    protected readonly Mock<IUnitOfWork> _unitOfWorkMock;
    protected readonly CancellationToken _cancellationToken;

    public CommandHandlerBaseTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cancellationToken = CancellationToken.None;
    }
}