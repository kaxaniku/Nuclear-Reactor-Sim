using System.Linq.Expressions;
using Moq;
using NuclearApp.Features.ReactorGrids;
using NuclearApp.Features.ReactorGrids.Handlers.CommandHandlers;
using NuclearDomain.Entities;

namespace NuclearUnitTests.ReactorGridUTests.CommandTests;

public class SetReactorWatchStateCommandHandlerTests : CommandHandlerBaseTests
{
    [Fact]
    public async Task Handle_ShouldSetIsMonitoredCorrectly()
    {
        var reactorGridId = 1;
        var isMonitored = true;
        var request = new SetReactorWatchStateCommand(reactorGridId, isMonitored);

        var grid = new ReactorGrid { Id = reactorGridId, Name = "Test Grid", IsMonitored = false };

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
            It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new List<ReactorGrid> { grid });

        var handler = new SetReactorWatchStateCommandHandler(_unitOfWorkMock.Object);

        await handler.Handle(request, _cancellationToken);

        Assert.True(grid.IsMonitored);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenReactorGridNotFound()
    {
        var reactorGridId = 1;
        var isMonitored = true;
        var request = new SetReactorWatchStateCommand(reactorGridId, isMonitored);

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
            It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new List<ReactorGrid>());

        var handler = new SetReactorWatchStateCommandHandler(_unitOfWorkMock.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(request, _cancellationToken));
    }
}