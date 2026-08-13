using System.Linq.Expressions;
using Moq;
using NuclearApp.Features.GridCells;
using NuclearApp.Features.GridCells.Handlers.CommandHandlers;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;
using NuclearDomain.Entities.Telemetries;

namespace NuclearUnitTests.ReactorCellUTests.CommandTests;

public class ToggleAllFuelRodsHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ToggleAllFuelRodsHandler _handler;

    public ToggleAllFuelRodsHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new ToggleAllFuelRodsHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldToggleAllFuelRods_WhenValidRequest()
    {
        // Arrange
        var request = new ToggleAllFuelRodsCommand(1);

        var reactorGrid = new ReactorGrid
        {
            Id = 1,
            Cells = new List<Cell>
            {
                new Cell { ColumnType = ColumnType.FuelChannel, Telemetry = new FuelChannelTelemetryDto { IsOnline = true } },
                new Cell { ColumnType = ColumnType.FuelChannel, Telemetry = new FuelChannelTelemetryDto { IsOnline = false } }
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
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        var cells = reactorGrid.Cells.Where(c => c.ColumnType == ColumnType.FuelChannel).ToList();
        var tele1 = cells[0].Telemetry as FuelChannelTelemetryDto;
        var tele2 = cells[1].Telemetry as FuelChannelTelemetryDto;
        Assert.Equal(2, cells.Count);
        Assert.False(tele1!.IsOnline);
        Assert.True(tele2!.IsOnline);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenReactorGridNotFound()
    {
        // Arrange
        var request = new ToggleAllFuelRodsCommand(1);

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>[]>()))
            .ReturnsAsync(new List<ReactorGrid>());

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenNoFuelChannels()
    {
        // Arrange
        var request = new ToggleAllFuelRodsCommand(1);

        var reactorGrid = new ReactorGrid { Id = 1, Cells = new List<Cell>() };

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>[]>()))
            .ReturnsAsync(new List<ReactorGrid> { reactorGrid });

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenInvalidTelemetryType()
    {
        // Arrange
        var request = new ToggleAllFuelRodsCommand(1);

        var reactorGrid = new ReactorGrid
        {
            Id = 1,
            Cells = new List<Cell>
            {
                new Cell { ColumnType = ColumnType.FuelChannel, Telemetry = new SteamChannelTelemetryDto() }
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
