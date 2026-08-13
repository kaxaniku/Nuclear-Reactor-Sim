using MediatR;
using NuclearApp.Interfaces.Repositories;

namespace NuclearApp.Behaviors;

public class ReactorValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;

    public ReactorValidationBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IRequiresValidReactor reactorCommand)
        {
            var grids = await _unitOfWork.ReactorGridRepository.QueryAsync(
                g => g.Id == reactorCommand.ReactorGridId,
                cancellationToken
            );

            var grid = grids.FirstOrDefault()
                ?? throw new KeyNotFoundException($"Reactor grid with ID {reactorCommand.ReactorGridId} was not found.");

            if (!grid.IsValid)
            {
                throw new InvalidOperationException(
                    $"Operation blocked: Reactor grid {reactorCommand.ReactorGridId} is currently invalid. " +
                    "Please run reactor validation before making structural changes."
                );
            }
        }

        return await next();
    }
}
