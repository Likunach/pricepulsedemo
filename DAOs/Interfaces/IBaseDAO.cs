using System.Linq.Expressions;

namespace PricePulse.DAOs.Interfaces
{
    public interface IBaseDAO<T> where T : class
    {
        // Create operations
        Task<T> CreateAsync(T entity);
        Task<IEnumerable<T>> CreateManyAsync(IEnumerable<T> entities);

        // Read operations
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FindFirstAsync(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        // Update operations
        Task<T> UpdateAsync(T entity);
        Task<IEnumerable<T>> UpdateManyAsync(IEnumerable<T> entities);

        // Delete operations
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteAsync(T entity);
        Task<int> DeleteManyAsync(Expression<Func<T, bool>> predicate);

        // Pagination
        Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate);

        // Ordering
        Task<IEnumerable<T>> GetOrderedAsync<TKey>(Expression<Func<T, TKey>> orderBy, bool ascending = true);
        Task<IEnumerable<T>> GetOrderedAsync<TKey>(Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> orderBy, bool ascending = true);
    }
}
