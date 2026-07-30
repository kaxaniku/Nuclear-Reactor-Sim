using System.Linq.Expressions;
using Moq;
using NuclearApp.Features.ReactorGrids;
using NuclearApp.Features.ReactorGrids.Handlers.QueryHandlers;
using NuclearDomain.Entities;

namespace NuclearUnitTests;

public class GetReactorGridIdByNameHandlerTests : QueryHandlerBaseTests
{
    [Fact]
    public async Task Handle_ShouldReturnCorrectReactorGridId()
    {
        var reactorGrid = new ReactorGrid { Id = 1, Name = "Test Grid", ActivityInfo = new ActivityInfo { IsActive = true } };

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(It.IsAny<Expression<Func<ReactorGrid, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReactorGrid> { reactorGrid });

        var handler = new GetReactorGridIdByNameHandler(_unitOfWorkMock.Object);

        var result = await handler.Handle(new GetReactorGridIdByNameQuery("Test Grid"), _cancellationToken);

        Assert.Equal(reactorGrid.Id, result);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenReactorGridNotFound()
    {
        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(It.IsAny<Expression<Func<ReactorGrid, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReactorGrid>());

        var handler = new GetReactorGridIdByNameHandler(_unitOfWorkMock.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(new GetReactorGridIdByNameQuery("Test Grid"), _cancellationToken));
    }
}