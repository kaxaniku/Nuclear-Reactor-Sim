using Moq;
using NuclearApp.Features.ReactorGrids;
using NuclearApp.Features.ReactorGrids.Handlers.CommandHandlers;
using NuclearDomain.Entities;

namespace NuclearUnitTests.ReactorGridUTests.CommandTests;

public class CreateReactorCommandHandlerTests : CommandHandlerBaseTests
{
    [Fact]
    public async Task Handle_ShouldCreateReactorGrid()
    {
        var request = new CreateReactorCommand("Test Reactor");
        var handler = new CreateReactorCommandHandler(_unitOfWorkMock.Object);

        _unitOfWorkMock.Setup(uow => uow.ReactorGridRepository.InsertAsync(
            It.IsAny<ReactorGrid>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReactorGrid grid, CancellationToken ct) => grid);
        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await handler.Handle(request, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal("Test Reactor", result.Name);
        Assert.Empty(result.Cells);
        Assert.Equal(0, result.TotalRows);
        Assert.Equal(0, result.TotalColumns);
    }
}
