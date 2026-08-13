using System.Linq.Expressions;
using Moq;
using NuclearApp.Features.ReactorGrids;
using NuclearApp.Features.ReactorGrids.Handlers.CommandHandlers;
using NuclearApp.Interfaces.Services;
using NuclearDomain.Entities;
using NuclearDomain.Entities.Telemetries;

namespace NuclearUnitTests.ReactorGridUTests.CommandTests;

public class ProcessReactorTickCommandHandlerTests : CommandHandlerBaseTests
{
    private readonly Mock<IReactorPhysicsEngine> _physicsEngineMock;

    public ProcessReactorTickCommandHandlerTests()
    {
        _physicsEngineMock = new Mock<IReactorPhysicsEngine>();
    }

    [Fact]
    public async Task Handle_ShouldProcessPhysicsTickAndSaveChanges()
    {
        var reactorGridId = 1;
        var deltaTimeSeconds = 60.0;

        var request = new ProcessReactorTickCommand(reactorGridId, deltaTimeSeconds);

        var grid = new ReactorGrid
        {
            Id = reactorGridId,
            Name = "Test Grid",
            IsMonitored = true,
            IsValid = true,
            Cells = new List<Cell>
            {
                new Cell { ColumnType = ColumnType.FuelChannel, Telemetry = new FuelChannelTelemetryDto { IsOnline = true, Status = FuelRodStatus.Nominal } }
            }
        };

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
            It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<Expression<Func<ReactorGrid, object>>>()
        )).ReturnsAsync(new List<ReactorGrid> { grid });

        var handler = new ProcessReactorTickCommandHandler(_unitOfWorkMock.Object, _physicsEngineMock.Object);

        await handler.Handle(request, _cancellationToken);

        _physicsEngineMock.Verify(pe => pe.ProcessPhysicsTick(grid, deltaTimeSeconds), Times.Once);
        _unitOfWorkMock.Verify(u => u.CellRepository.MarkRangeModified(grid.Cells), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotProcessPhysicsTickWhenNotMonitored()
    {
        var reactorGridId = 1;
        var deltaTimeSeconds = 60.0;

        var request = new ProcessReactorTickCommand(reactorGridId, deltaTimeSeconds);

        var grid = new ReactorGrid
        {
            Id = reactorGridId,
            Name = "Test Grid",
            IsMonitored = false,
            IsValid = true,
            Cells = new List<Cell>
        {
            new Cell { ColumnType = ColumnType.FuelChannel, Telemetry = new FuelChannelTelemetryDto { IsOnline = true, Status = FuelRodStatus.Nominal } }
        }
        };

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
            It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<Expression<Func<ReactorGrid, object>>[]>()
        ))
        .ReturnsAsync((
            Expression<Func<ReactorGrid, bool>> predicate,
            CancellationToken token,
            Expression<Func<ReactorGrid, object>>[] includes) =>
        {
            var compiled = predicate.Compile();
            var allGrids = new List<ReactorGrid> { grid };
            return allGrids.Where(compiled).ToList();
        });

        var handler = new ProcessReactorTickCommandHandler(_unitOfWorkMock.Object, _physicsEngineMock.Object);

        await handler.Handle(request, _cancellationToken);

        _physicsEngineMock.Verify(pe => pe.ProcessPhysicsTick(It.IsAny<ReactorGrid>(), It.IsAny<double>()), Times.Never);

        _unitOfWorkMock.Verify(u => u.CellRepository.MarkRangeModified(It.IsAny<IEnumerable<Cell>>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotProcessPhysicsTickWhenInvalid()
    {
        var reactorGridId = 1;
        var deltaTimeSeconds = 60.0;

        var request = new ProcessReactorTickCommand(reactorGridId, deltaTimeSeconds);

        var grid = new ReactorGrid
        {
            Id = reactorGridId,
            Name = "Test Grid",
            IsMonitored = true,
            IsValid = false,
            Cells = new List<Cell>
            {
                new Cell { ColumnType = ColumnType.FuelChannel, Telemetry = new FuelChannelTelemetryDto { IsOnline = true, Status = FuelRodStatus.Nominal } }
            }
        };

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
            It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<Expression<Func<ReactorGrid, object>>[]>()
        ))
        .ReturnsAsync((
            Expression<Func<ReactorGrid, bool>> predicate,
            CancellationToken token,
            Expression<Func<ReactorGrid, object>>[] includes) =>
        {
            var compiled = predicate.Compile();
            var allGrids = new List<ReactorGrid> { grid };
            return allGrids.Where(compiled).ToList();
        });

        var handler = new ProcessReactorTickCommandHandler(_unitOfWorkMock.Object, _physicsEngineMock.Object);

        await handler.Handle(request, _cancellationToken);

        _physicsEngineMock.Verify(pe => pe.ProcessPhysicsTick(It.IsAny<ReactorGrid>(), It.IsAny<double>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenReactorGridNotFound()
    {
        var reactorGridId = 1;
        var deltaTimeSeconds = 60.0;

        var request = new ProcessReactorTickCommand(reactorGridId, deltaTimeSeconds);

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
            It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<Expression<Func<ReactorGrid, object>>>()
        )).ReturnsAsync(new List<ReactorGrid>());

        var handler = new ProcessReactorTickCommandHandler(_unitOfWorkMock.Object, _physicsEngineMock.Object);

        await Assert.ThrowsAsync<Exception>(() => handler.Handle(request, _cancellationToken));
    }
}