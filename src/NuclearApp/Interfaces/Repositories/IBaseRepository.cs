using System.Linq.Expressions;

namespace NuclearApp.Interfaces.Repositories;

public interface IBaseRepository<T> where T : class
{
    T? GetById(int id);

    IQueryable<T> Query(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);

    IQueryable<T> Query(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize, params Expression<Func<T, object>>[] includes);

    void Insert(T entity);
    void Update(T entity);
    void Delete(T entity);

    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken, params Expression<Func<T, object>>[] includes);

    Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize, CancellationToken cancellationToken, params Expression<Func<T, object>>[] includes);

    Task<T> InsertAsync(T entity, CancellationToken cancellationToken);
    Task UpdateAsync(T entity);

    void Dispose();

    ValueTask DisposeAsync();
}
