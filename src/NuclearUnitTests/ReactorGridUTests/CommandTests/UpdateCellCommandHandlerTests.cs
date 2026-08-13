using System.Data.Common;
using System.Linq.Expressions;
using Moq;
using NuclearApp.DTOs;
using NuclearApp.Features.ReactorGrids;
using NuclearApp.Features.ReactorGrids.Handlers.CommandHandlers;
using NuclearDomain.Entities;

namespace NuclearUnitTests.ReactorGridUTests.CommandTests;

public class UpdateCellCommandHandlerTests : CommandHandlerBaseTests
{
    [Fact]
    public async Task Handle_ShouldUpdateCell()
    {
        var reactorGrid = new ReactorGrid { Id = 1, Cells = new List<Cell> { new Cell { X = 0, Y = 0, ColumnType = ColumnType.SteamChannel } } };
        var request = new UpdateCellCommand(1, new ConfigureCellCommandDto { X = 0, Y = 0, NewColumnType = ColumnType.FuelChannel });
        var handler = new UpdateCellCommandHandler(_unitOfWorkMock.Object);

        _unitOfWorkMock.Setup(uow => uow.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>>()))
            .ReturnsAsync(new List<ReactorGrid> { reactorGrid });
        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await handler.Handle(request, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(0, result.X);
        Assert.Equal(0, result.Y);
        Assert.Equal(ColumnType.FuelChannel, result.ColumnType);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenReactorGridNotFound()
    {
        var request = new UpdateCellCommand(1, new ConfigureCellCommandDto { X = 0, Y = 0, NewColumnType = ColumnType.FuelChannel });
        var handler = new UpdateCellCommandHandler(_unitOfWorkMock.Object);

        _unitOfWorkMock.Setup(uow => uow.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>>()))
            .ReturnsAsync(new List<ReactorGrid>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(request, _cancellationToken));
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenCellNotFound()
    {
        var reactorGrid = new ReactorGrid { Id = 1 };
        var request = new UpdateCellCommand(1, new ConfigureCellCommandDto { X = 0, Y = 0, NewColumnType = 0 });
        var handler = new UpdateCellCommandHandler(_unitOfWorkMock.Object);

        _unitOfWorkMock.Setup(uow => uow.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>>()))
            .ReturnsAsync(new List<ReactorGrid>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(request, _cancellationToken));
    }
}
