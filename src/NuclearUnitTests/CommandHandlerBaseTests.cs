using Moq;
using NuclearApp.Interfaces.Repositories;

namespace NuclearUnitTests;

public abstract class CommandHandlerBaseTests
{
    protected readonly Mock<IUnitOfWork> _unitOfWorkMock;
    protected readonly CancellationToken _cancellationToken;

    protected CommandHandlerBaseTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>
        {
            DefaultValue = DefaultValue.Mock
        };
        _cancellationToken = CancellationToken.None;
    }
}