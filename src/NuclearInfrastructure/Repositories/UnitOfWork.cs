using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NuclearApp.Interfaces.Repositories;

namespace NuclearInfrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ReactorDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    private readonly Lazy<IReactorGridRepository> _reactorGrid;
    private readonly Lazy<ICellRepository> _cell;

    public IReactorGridRepository ReactorGridRepository => CheckDisposedAndGet(_reactorGrid);
    public ICellRepository CellRepository => CheckDisposedAndGet(_cell);

    public UnitOfWork(ReactorDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));

        _reactorGrid = new Lazy<IReactorGridRepository>(() => new ReactorGridRepository(_context));
        _cell = new Lazy<ICellRepository>(() => new CellRepository(_context));

    }

    public int SaveChanges()
    {
        ThrowIfDisposed();
        return _context.SaveChanges();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void BeginTransaction()
    {
        ThrowIfDisposed();
        if (_transaction != null)
            throw new ArgumentException("Transaction has already started");

        _transaction = _context.Database.BeginTransaction();
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        if (_transaction != null)
            throw new ArgumentException("Transaction has already started");

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

    }

    public void Commit()
    {
        ThrowIfDisposed();
        if (_transaction == null)
            throw new ArgumentException("Transaction has not started");

        _transaction?.Commit();
        _transaction?.Dispose();
        _transaction = null;
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        if (_transaction == null)
            throw new ArgumentException("Transaction has not started");

        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;

    }

    public void Rollback()
    {
        ThrowIfDisposed();
        if (_transaction == null)
            throw new ArgumentException("Transaction has not started");

        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
    }

    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        if (_transaction == null)
            throw new ArgumentException("Transaction has not started");

        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    public void ClearTracker()
    {
        _context.ChangeTracker.Clear();
    }

    private T CheckDisposedAndGet<T>(Lazy<T> lazy)
    {
        ThrowIfDisposed();
        return lazy.Value;
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            if (_transaction != null)
            {
                _transaction.DisposeAsync();
                _transaction = null;
            }

            if (_reactorGrid.IsValueCreated)
                _reactorGrid.Value.Dispose();

            if (_cell.IsValueCreated)
                _cell.Value.Dispose();
        }

        _disposed = true;
    }

    private async ValueTask DisposeAsyncCore()
    {
        if (!_disposed)
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }

            if (_reactorGrid.IsValueCreated)
                await _reactorGrid.Value.DisposeAsync();

            if (_cell.IsValueCreated)
                await _cell.Value.DisposeAsync();


            _disposed = true;
        }
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, GetType());

    ~UnitOfWork() => Dispose(false);
}
