using System.Linq.Expressions;
using Moq;
using NuclearApp.Features.GridCells;
using NuclearApp.Features.GridCells.Handlers.CommandHandlers;
using NuclearApp.Interfaces.Repositories;
using NuclearDomain.Entities;
using NuclearDomain.Entities.Telemetries;

namespace NuclearUnitTests.ReactorCellUTests.CommandTests;

public class MoveAllControlRodsCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly MoveAllControlRodsCommandHandler _handler;

    public MoveAllControlRodsCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new MoveAllControlRodsCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldMoveAllControlRods_WhenValidRequest()
    {
        // Arrange
        var request = new MoveAllControlRodsCommand(1, 50);

        var reactorGrid = new ReactorGrid
        {
            Id = 1,
            Cells = new List<Cell>
            {
                new Cell { ColumnType = ColumnType.ControlRods, X = 0, Y = 0, Telemetry = new ControlRodsTelemetryDto() },
                new Cell { ColumnType = ColumnType.ControlRods, X = 1, Y = 0, Telemetry = new ControlRodsTelemetryDto() }
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
            var telemetry = (ControlRodsTelemetryDto)cell.Telemetry;
            Assert.Equal(0.5, telemetry.TargetInsertionPercentage);
        }
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenReactorGridNotFound()
    {
        // Arrange
        var request = new MoveAllControlRodsCommand(1, 50);

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>[]>()))
            .ReturnsAsync(new List<ReactorGrid>());

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenNoControlRods()
    {
        // Arrange
        var request = new MoveAllControlRodsCommand(1, 50);

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
