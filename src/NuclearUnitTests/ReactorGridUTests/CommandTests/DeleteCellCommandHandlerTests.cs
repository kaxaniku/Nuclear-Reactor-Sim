using System.Linq.Expressions;
using Moq;
using NuclearApp.Features.ReactorGrids;
using NuclearApp.Features.ReactorGrids.Handlers.CommandHandlers;
using NuclearDomain.Entities;

namespace NuclearUnitTests.ReactorGridUTests.CommandTests;

public class DeleteCellCommandHandlerTests : CommandHandlerBaseTests
{
    [Fact]
    public async Task Handle_ShouldDeleteCell()
    {
        var reactorGrid = new ReactorGrid { Id = 1, Cells = new List<Cell> { new Cell { X = 0, Y = 0 } } };
        var request = new DeleteCellCommand(1, 0, 0);
        var handler = new DeleteCellCommandHandler(_unitOfWorkMock.Object);

        _unitOfWorkMock.Setup(uow => uow.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>>()
            ))
            .ReturnsAsync(new List<ReactorGrid> { reactorGrid });
        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await handler.Handle(request, _cancellationToken);

        Assert.Empty(reactorGrid.Cells);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenReactorGridNotFound()
    {
        var request = new DeleteCellCommand(1, 0, 0);
        var handler = new DeleteCellCommandHandler(_unitOfWorkMock.Object);

        _unitOfWorkMock.Setup(uow => uow.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>>()
            ))
            .ReturnsAsync(new List<ReactorGrid>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(request, _cancellationToken));
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenCellNotFound()
    {
        var reactorGrid = new ReactorGrid { Id = 1 };
        var request = new DeleteCellCommand(1, 0, 0);
        var handler = new DeleteCellCommandHandler(_unitOfWorkMock.Object);

        _unitOfWorkMock.Setup(uow => uow.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>>()
            ))
            .ReturnsAsync(new List<ReactorGrid> { reactorGrid });

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(request, _cancellationToken));
    }
}
