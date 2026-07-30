using System.Linq.Expressions;
using Moq;
using NuclearApp.Features.ReactorGrids;
using NuclearApp.Features.ReactorGrids.Handlers.QueryHandlers;
using NuclearDomain.Entities;

namespace NuclearUnitTests;

public class GetAllReactorsHandlerTests : QueryHandlerBaseTests
{
    [Fact]
    public async Task Handle_ShouldReturnAllReactorGrids()
    {
        var reactorGrids = new List<ReactorGrid>
            {
                new ReactorGrid { Id = 1, Name = "Grid 1" },
                new ReactorGrid { Id = 2, Name = "Grid 2" }
            };

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>[]>()))
            .ReturnsAsync(reactorGrids);

        var handler = new GetAllReactorGridsHandler(_unitOfWorkMock.Object);

        var result = await handler.Handle(new GetAllReactorGridsQuery(), _cancellationToken);

        Assert.Equal(2, result.Count());
        Assert.Contains(result, g => g.Id == 1 && g.Name == "Grid 1");
        Assert.Contains(result, g => g.Id == 2 && g.Name == "Grid 2");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoReactors()
    {
        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.QueryAsync(
                It.IsAny<Expression<Func<ReactorGrid, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<ReactorGrid, object>>[]>()))
            .ReturnsAsync(new List<ReactorGrid>());

        var handler = new GetAllReactorGridsHandler(_unitOfWorkMock.Object);

        var result = await handler.Handle(new GetAllReactorGridsQuery(), _cancellationToken);

        Assert.Empty(result);
    }
}
