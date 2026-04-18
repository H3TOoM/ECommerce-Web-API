namespace ShopAPI.Repoistires.Base
{
    /// <summary>
    /// Generic repository interface with support for specifications and complex queries
    /// </summary>
    public interface IMainRepository<T> where T : class
    {
        /// <summary>
        /// Get all entities
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Get entity by ID
        /// </summary>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Get entities by specification (with eager loading, filtering, sorting, pagination)
        /// </summary>
        Task<IEnumerable<T>> GetBySpecificationAsync(Specification<T> specification);

        /// <summary>
        /// Get count of entities matching specification
        /// </summary>
        Task<int> GetCountBySpecificationAsync(Specification<T> specification);

        /// <summary>
        /// Check if entity exists by condition
        /// </summary>
        Task<bool> AnyAsync(Func<T, bool> predicate);

        /// <summary>
        /// Get first or default by specification
        /// </summary>
        Task<T?> GetFirstOrDefaultAsync(Specification<T> specification);

        /// <summary>
        /// Create entity
        /// </summary>
        Task<T> CreateAsync(T entity);

        /// <summary>
        /// Update entity
        /// </summary>
        Task<T?> UpdateAsync(int id, T entity);

        /// <summary>
        /// Delete entity
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Create multiple entities
        /// </summary>
        Task<IEnumerable<T>> CreateRangeAsync(IEnumerable<T> entities);
    }
}
