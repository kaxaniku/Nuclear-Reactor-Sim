using System.Linq.Expressions;
using Moq;
using NuclearApp.Features.GridCells;
using NuclearApp.Features.GridCells.Handlers.CommandHandlers;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;
using NuclearDomain.Entities.Telemetries;

namespace NuclearUnitTests.ReactorCellUTests.CommandTests;

public class ConfigureAllSteamChannelsCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ConfigureAllSteamChannelsCommandHandler _handler;

    public ConfigureAllSteamChannelsCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new ConfigureAllSteamChannelsCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldConfigureAllSteamChannels_WhenValidRequest()
    {
        // Arrange
        var request = new ConfigureAllSteamChannelsCommand(1, SteamType.Dense, 0.8);

        var reactorGrid = new ReactorGrid
        {
            Id = 1,
            Cells = new List<Cell>
        {
            new Cell { ColumnType = ColumnType.SteamChannel, X = 0, Y = 0, Telemetry = new SteamChannelTelemetryDto() },
            new Cell { ColumnType = ColumnType.SteamChannel, X = 1, Y = 0, Telemetry = new SteamChannelTelemetryDto() }
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
        foreach (var cell in reactorGrid.Cells)
        {
            var telemetry = (SteamChannelTelemetryDto)cell.Telemetry;
            Assert.Equal(SteamType.Dense, telemetry.SteamType);
            Assert.Equal(0.8, telemetry.FlowRateThrottling);
        }

        cellRepositoryMock.Verify(r => r.MarkModified(It.IsAny<Cell>()), Times.Exactly(2));
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenReactorGridNotFound()
    {
        // Arrange
        var request = new ConfigureAllSteamChannelsCommand(1, SteamType.Dense, 0.8);

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>[]>()))
            .ReturnsAsync(new List<ReactorGrid>());

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenNoSteamChannelsFound()
    {
        // Arrange
        var request = new ConfigureAllSteamChannelsCommand(1, SteamType.Dense, 0.8);

        var reactorGrid = new ReactorGrid { Id = 1, Cells = new List<Cell>() };

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>[]>()))
            .ReturnsAsync(new List<ReactorGrid> { reactorGrid });

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(request, CancellationToken.None));
    }
}