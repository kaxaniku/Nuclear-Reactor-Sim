namespace NuclearApp.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        ICellRepository CellRepository { get; }
        IConfigureCellCommandRepository ConfigureCellCommandRepository { get; }
        IMoveControlRodCommandRepository MoveControlRodCommandRepository { get; }
        IReactorGridRepository ReactorGridRepository { get; }
        IReactorOverviewRepository ReactorOverviewRepository { get; }
        IScramReactorCommandRepository ScramReactorCommandRepository { get; }

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
}