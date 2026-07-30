using Moq;
using NuclearApp.Interfaces.Repositories;

namespace NuclearUnitTests;

public abstract class QueryHandlerBaseTests
{
    protected readonly Mock<IUnitOfWork> _unitOfWorkMock;
    protected readonly CancellationToken _cancellationToken;

    protected QueryHandlerBaseTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>
        {
            DefaultValue = DefaultValue.Mock
        };
        _cancellationToken = CancellationToken.None;
    }
}