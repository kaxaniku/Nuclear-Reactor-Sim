using System.Linq.Expressions;
using Moq;
using NuclearApp.DTOs;
using NuclearApp.Features.ReactorGrids;
using NuclearApp.Features.ReactorGrids.Handlers.CommandHandlers;
using NuclearDomain.Entities;

namespace NuclearUnitTests.ReactorGridUTests.CommandTests;

public class InsertCellCommandHandlerTests : CommandHandlerBaseTests
{
    [Fact]
    public async Task Handle_ShouldInsertCell()
    {
        var reactorGrid = new ReactorGrid { Id = 1, Cells = new List<Cell>() };
        var request = new InsertCellCommand(1, new ConfigureCellCommandDto { X = 0, Y = 0, NewColumnType = ColumnType.Structural });
        var handler = new InsertCellCommandHandler(_unitOfWorkMock.Object);

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
        Assert.Equal(ColumnType.Structural, result.ColumnType);
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenCellAlreadyExists()
    {
        var reactorGrid = new ReactorGrid { Id = 1, Cells = new List<Cell> { new Cell { X = 0, Y = 0 } } };
        var request = new InsertCellCommand(1, new ConfigureCellCommandDto { X = 0, Y = 0, NewColumnType = ColumnType.Structural });
        var handler = new InsertCellCommandHandler(_unitOfWorkMock.Object);

        _unitOfWorkMock.Setup(uow => uow.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>>()))
            .ReturnsAsync(new List<ReactorGrid> { reactorGrid });

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(request, _cancellationToken));
    }
}