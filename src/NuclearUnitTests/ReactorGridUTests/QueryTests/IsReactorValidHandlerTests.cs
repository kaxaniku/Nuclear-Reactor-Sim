using System.Linq.Expressions;
using Moq;
using NuclearApp.Features.ReactorGrids;
using NuclearApp.Features.ReactorGrids.Handlers.QueryHandlers;
using NuclearDomain.Entities;

namespace NuclearUnitTests;

public class IsReactorValidHandlerTests : QueryHandlerBaseTests
{
    [Fact]
    public async Task Handle_ShouldReturnTrue_WhenReactorIsValid()
    {
        var reactorGridId = 1;
        var cells = new List<Cell>
            {
                new Cell { X = 0, Y = 0 },
                new Cell { X = 1, Y = 0 }
            };
        var reactorGrid = new ReactorGrid { Id = reactorGridId, Cells = cells };

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>[]>()))
            .ReturnsAsync(new List<ReactorGrid> { reactorGrid });

        var handler = new IsReactorValidHandler(_unitOfWorkMock.Object);

        var result = await handler.Handle(new IsReactorValidQuery(reactorGridId), _cancellationToken);

        Assert.True(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenReactorIsInvalid()
    {
        var reactorGridId = 1;
        var cells = new List<Cell>
            {
                new Cell { X = 0, Y = 0 },
                new Cell { X = 2, Y = 0 } // Isolated cell
            };
        var reactorGrid = new ReactorGrid { Id = reactorGridId, Cells = cells };

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>[]>()))
            .ReturnsAsync(new List<ReactorGrid> { reactorGrid });

        var handler = new IsReactorValidHandler(_unitOfWorkMock.Object);

        var result = await handler.Handle(new IsReactorValidQuery(reactorGridId), _cancellationToken);

        Assert.False(result);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenReactorGridNotFound()
    {
        var reactorGridId = 1;

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>[]>()))
            .ReturnsAsync(new List<ReactorGrid>());

        var handler = new IsReactorValidHandler(_unitOfWorkMock.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(new IsReactorValidQuery(reactorGridId), _cancellationToken));
    }
}
