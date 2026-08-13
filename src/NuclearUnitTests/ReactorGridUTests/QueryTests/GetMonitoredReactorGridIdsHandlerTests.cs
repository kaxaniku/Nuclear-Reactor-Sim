using System.Linq.Expressions;
using Moq;
using NuclearApp.Features.ReactorGrids;
using NuclearApp.Features.ReactorGrids.Handlers.QueryHandlers;
using NuclearDomain.Entities;

namespace NuclearUnitTests;

public class GetMonitoredReactorGridIdsHandlerTests : QueryHandlerBaseTests
{
    [Fact]
    public async Task Handle_ShouldReturnCorrectMonitoredReactorGridIds()
    {
        var monitoredReactors = new List<ReactorGrid>
        {
            new ReactorGrid { Id = 1, Name = "Test Grid 1", IsMonitored = true },
            new ReactorGrid { Id = 2, Name = "Test Grid 2", IsMonitored = true }
        };

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
            It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(monitoredReactors);

        var handler = new GetMonitoredReactorGridIdsHandler(_unitOfWorkMock.Object);

        var result = await handler.Handle(new GetMonitoredReactorGridIdsQuery(), _cancellationToken);

        Assert.Equal(monitoredReactors.Select(g => g.Id).ToList(), result);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoMonitoredReactorGridsFound()
    {
        var monitoredReactors = new List<ReactorGrid>();

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
            It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(monitoredReactors);

        var handler = new GetMonitoredReactorGridIdsHandler(_unitOfWorkMock.Object);

        var result = await handler.Handle(new GetMonitoredReactorGridIdsQuery(), _cancellationToken);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectMonitoredReactorGridIds_WhenSomeAreNotMonitored()
    {
        var monitoredReactors = new List<ReactorGrid>
        {
            new ReactorGrid { Id = 1, Name = "Test Grid 1", IsMonitored = true },
            new ReactorGrid { Id = 2, Name = "Test Grid 2", IsMonitored = false }
        };

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
            It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
            It.IsAny<CancellationToken>()
        ))
        .ReturnsAsync(() => monitoredReactors.Where(g => g.IsMonitored).ToList());

        var handler = new GetMonitoredReactorGridIdsHandler(_unitOfWorkMock.Object);

        var result = await handler.Handle(new GetMonitoredReactorGridIdsQuery(), _cancellationToken);

        Assert.Equal(new List<int> { 1 }, result);
    }
}