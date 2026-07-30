using System.Linq.Expressions;
using Moq;
using NuclearApp.Features.ReactorGrids;
using NuclearApp.Features.ReactorGrids.Handlers.QueryHandlers;
using NuclearDomain.Entities;

namespace NuclearUnitTests;

public class GetCellByIdHandlerTests : QueryHandlerBaseTests
{
    [Fact]
    public async Task Handle_ShouldReturnCorrectCell()
    {
        var reactorGridId = 1;
        var cells = new List<Cell>
            {
                new Cell { Id = 1, X = 0, Y = 0 },
                new Cell { Id = 2, X = 1, Y = 0 }
            };
        var reactorGrid = new ReactorGrid { Id = reactorGridId, Cells = cells };

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(It.IsAny<Expression<Func<ReactorGrid, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<ReactorGrid, object>>[]>()))
            .ReturnsAsync(new List<ReactorGrid> { reactorGrid });

        var handler = new GetCellByIdHandler(_unitOfWorkMock.Object);

        var result = await handler.Handle(new GetCellByIdQuery(reactorGridId, 1), _cancellationToken);

        Assert.Equal(cells[0], result);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenReactorGridNotFound()
    {
        var reactorGridId = 1;

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(It.IsAny<Expression<Func<ReactorGrid, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<ReactorGrid, object>>[]>()))
            .ReturnsAsync(new List<ReactorGrid>());

        var handler = new GetCellByIdHandler(_unitOfWorkMock.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(new GetCellByIdQuery(reactorGridId, 1), _cancellationToken));
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenCellNotFound()
    {
        var reactorGridId = 1;
        var cells = new List<Cell>
            {
                new Cell { Id = 1, X = 0, Y = 0 },
                new Cell { Id = 2, X = 1, Y = 0 }
            };
        var reactorGrid = new ReactorGrid { Id = reactorGridId, Cells = cells };

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(It.IsAny<Expression<Func<ReactorGrid, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<ReactorGrid, object>>[]>()))
            .ReturnsAsync(new List<ReactorGrid> { reactorGrid });

        var handler = new GetCellByIdHandler(_unitOfWorkMock.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(new GetCellByIdQuery(reactorGridId, 3), _cancellationToken));
    }
}
