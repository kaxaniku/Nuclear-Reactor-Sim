using Moq;
using NuclearApp.Features.ReactorGrids;
using NuclearApp.Features.ReactorGrids.Handlers.CommandHandlers;
using NuclearDomain.Entities;

namespace NuclearUnitTests.ReactorGridUTests.CommandTests;

public class DeleteReactorCommandHandlerTests : CommandHandlerBaseTests
{
    [Fact]
    public async Task Handle_ShouldDeleteReactor()
    {
        var reactorGrid = new ReactorGrid { Id = 1 };
        var request = new DeleteReactorCommand(1);
        var handler = new DeleteReactorCommandHandler(_unitOfWorkMock.Object);

        _unitOfWorkMock.Setup(uow => uow.ReactorGridRepository.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reactorGrid);
        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await handler.Handle(request, _cancellationToken);

        _unitOfWorkMock.Verify(uow => uow.ReactorGridRepository.Delete(reactorGrid), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenReactorNotFound()
    {
        var request = new DeleteReactorCommand(1);
        var handler = new DeleteReactorCommandHandler(_unitOfWorkMock.Object);

        _unitOfWorkMock.Setup(uow => uow.ReactorGridRepository.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReactorGrid?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(request, _cancellationToken));
    }
}
