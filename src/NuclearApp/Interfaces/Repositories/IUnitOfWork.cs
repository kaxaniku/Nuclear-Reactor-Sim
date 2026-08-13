namespace NuclearApp.Interfaces.Repositories;

public interface IUnitOfWork
{
    ICellRepository CellRepository { get; }
    IReactorGridRepository ReactorGridRepository { get; }

    void BeginTransaction();
    Task BeginTransactionAsync(CancellationToken cancellationToken);
    void ClearTracker();
    void Commit();
    Task CommitAsync(CancellationToken cancellationToken);
    void Dispose();
    ValueTask DisposeAsync();
    void Rollback();
    Task RollbackAsync(CancellationToken cancellationToken);
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
