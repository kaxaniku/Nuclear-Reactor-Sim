using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NuclearApp.Interfaces;
using NuclearApp.Interfaces.Repositories;
using Shared.Extensions;

namespace NuclearInfrastructure.Repositories;

internal abstract class BaseRepository<T> : IDisposable, IAsyncDisposable, IBaseRepository<T> where T : class
{
    private readonly DbSet<T> _dbSet;
    private bool _disposed = false;

    protected BaseRepository(ReactorDbContext context)
    {
        _dbSet = context.Set<T>();
    }

    public T? GetById(int id)
    {
        ThrowIfDisposed();
        return _dbSet.Find(id);
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        if (typeof(IDisable).IsAssignableFrom(typeof(T)))
        {
            return await _dbSet
                .FirstOrDefaultAsync(e =>
                    EF.Property<int>(e, "Id") == id &&
                    ((IDisable)e).Activity.IsActive != false,
                    cancellationToken);
        }

        return await _dbSet.FindAsync(id, cancellationToken);
    }

    public IQueryable<T> Query(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        ThrowIfDisposed();

        IQueryable<T> query = _dbSet;
        includes.ForEach(include => query = query.Include(include));

        return query.Where(predicate);
    }

    public IQueryable<T> Query(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize, params Expression<Func<T, object>>[] includes)
    {
        ThrowIfDisposed();

        if (pageNumber < 1)
            throw new ArgumentException("Page number must be >= 1.", nameof(pageNumber));

        const int defaultPageSize = 10;
        if (pageSize < 1) pageSize = defaultPageSize;

        int skip = (pageNumber - 1) * pageSize;

        return Query(predicate, includes)
            .Skip(skip)
            .Take(pageSize);
    }

    public async Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken, params Expression<Func<T, object>>[] includes)
    {
        ThrowIfDisposed();

        IQueryable<T> query = _dbSet;
        includes.ForEach(include => query = query.Include(include));

        return await query.Where(predicate).ToListAsync();
    }

    public async Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize, CancellationToken cancellationToken, params Expression<Func<T, object>>[] includes)
    {
        ThrowIfDisposed();

        if (pageNumber < 1)
            throw new ArgumentException("Page number must be >= 1.", nameof(pageNumber));

        const int defaultPageSize = 10;
        if (pageSize < 1) pageSize = defaultPageSize;

        int skip = (pageNumber - 1) * pageSize;

        IQueryable<T> query = _dbSet;
        foreach (var include in includes)
            query = query.Include(include);

        return await query
            .Where(predicate)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public void Insert(T entity)
    {
        ThrowIfDisposed();
        _dbSet.Add(entity);
    }

    public async Task<T> InsertAsync(T entity, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public void Update(T entity)
    {
        ThrowIfDisposed();
        _dbSet.Update(entity);
    }

    public Task UpdateAsync(T entity)
    {
        ThrowIfDisposed();
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public void Delete(T entity)
    {
        if (entity is IDisable dEntity)
        {
            if (dEntity.Activity.IsActive)
            {
                dEntity.Activity.IsActive = false;
                _dbSet.Update(entity);
                return;
            }
            else
                throw new DbUpdateConcurrencyException("Entity is already disabled.");
        }

        _dbSet.Remove(entity);
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

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {

        }


        _disposed = true;
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (!_disposed)
        {

            _disposed = true;
        }
    }
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, GetType());

    ~BaseRepository() => Dispose(false);
}