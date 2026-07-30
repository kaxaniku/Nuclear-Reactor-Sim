using Moq;
using NuclearApp.Features.ReactorGrids;
using NuclearApp.Features.ReactorGrids.Handlers.QueryHandlers;
using NuclearDomain.Entities;

namespace NuclearUnitTests;

public class GetReactorByIdHandlerTests : QueryHandlerBaseTests
{
    [Fact]
    public async Task Handle_ShouldReturnCorrectReactorGrid()
    {
        var reactorGridId = 1;
        var reactorGrid = new ReactorGrid { Id = reactorGridId, Name = "Grid 1" };

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.GetByIdAsync(
                reactorGridId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(reactorGrid);

        var handler = new GetReactorGridByIdHandler(_unitOfWorkMock.Object);

        var result = await handler.Handle(new GetReactorGridByIdQuery(reactorGridId), _cancellationToken);

        Assert.Equal(reactorGrid, result);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenReactorGridNotFound()
    {
        var reactorGridId = 1;

        _unitOfWorkMock.Setup(u => u.ReactorGridRepository.GetByIdAsync(
            reactorGridId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync((ReactorGrid)null!);

        var handler = new GetReactorGridByIdHandler(_unitOfWorkMock.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(new GetReactorGridByIdQuery(reactorGridId), _cancellationToken));
    }
}