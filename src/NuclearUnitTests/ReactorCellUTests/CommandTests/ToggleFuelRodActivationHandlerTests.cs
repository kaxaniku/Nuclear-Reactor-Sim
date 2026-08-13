using System.Linq.Expressions;
using Moq;
using NuclearApp.Features.GridCells;
using NuclearApp.Features.GridCells.Handlers.CommandHandlers;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;
using NuclearDomain.Entities.Telemetries;

namespace NuclearUnitTests.ReactorCellUTests.CommandTests;

public class ToggleFuelRodActivationHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ToggleFuelRodActivationHandler _handler;

    public ToggleFuelRodActivationHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new ToggleFuelRodActivationHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldToggleFuelRodActivation_WhenValidRequest()
    {
        // Arrange
        var request = new ToggleFuelRodActivationCommand(1, 0, 0);

        var reactorGrid = new ReactorGrid
        {
            Id = 1,
            Cells = new List<Cell>
            {
                new Cell { X = 0, Y = 0, ColumnType = ColumnType.FuelChannel, Telemetry = new FuelChannelTelemetryDto { IsOnline = true } }
            }
        };

        var cellRepositoryMock = new Mock<ICellRepository>();

        _unitOfWorkMock
            .Setup(u => u.CellRepository)
            .Returns(cellRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(u => u.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>[]>()))
            .ReturnsAsync(new List<ReactorGrid> { reactorGrid });

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        var cell = reactorGrid.Cells.FirstOrDefault(c => c.X == request.X && c.Y == request.Y);
        var tele = cell!.Telemetry as FuelChannelTelemetryDto;
        Assert.NotNull(cell);
        Assert.Equal(ColumnType.FuelChannel, cell.ColumnType);
        Assert.False(tele!.IsOnline);
        Assert.False(result);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenReactorGridNotFound()
    {
        // Arrange
        var request = new ToggleFuelRodActivationCommand(1, 0, 0);

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>[]>()))
            .ReturnsAsync(new List<ReactorGrid>());

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenCellNotFound()
    {
        // Arrange
        var request = new ToggleFuelRodActivationCommand(1, 0, 0);

        var reactorGrid = new ReactorGrid { Id = 1, Cells = new List<Cell>() };

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>[]>()))
            .ReturnsAsync(new List<ReactorGrid> { reactorGrid });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenInvalidCellType()
    {
        // Arrange
        var request = new ToggleFuelRodActivationCommand(1, 0, 0);

        var reactorGrid = new ReactorGrid
        {
            Id = 1,
            Cells = new List<Cell>
            {
                new Cell { X = 0, Y = 0, ColumnType = ColumnType.SteamChannel, Telemetry = new SteamChannelTelemetryDto() }
            }
        };

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>[]>()))
            .ReturnsAsync(new List<ReactorGrid> { reactorGrid });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenInvalidTelemetryType()
    {
        // Arrange
        var request = new ToggleFuelRodActivationCommand(1, 0, 0);

        var reactorGrid = new ReactorGrid
        {
            Id = 1,
            Cells = new List<Cell>
            {
                new Cell { X = 0, Y = 0, ColumnType = ColumnType.FuelChannel, Telemetry = new SteamChannelTelemetryDto() }
            }
        };

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>[]>()))
            .ReturnsAsync(new List<ReactorGrid> { reactorGrid });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(request, CancellationToken.None));
    }
}