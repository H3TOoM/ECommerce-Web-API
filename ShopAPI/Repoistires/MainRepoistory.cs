using Microsoft.EntityFrameworkCore;
using ShopAPI.Data;
using ShopAPI.Helpers;
using ShopAPI.Repoistires.Base;

namespace ShopAPI.Repoistires
{
    /// <summary>
    /// Generic repository implementation with support for specifications and eager loading
    /// </summary>
    public class MainRepository<T> : IMainRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public MainRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        /// <summary>
        /// Get all entities (use with caution - may cause performance issues with large datasets)
        /// </summary>
        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        /// <summary>
        /// Get entity by ID
        /// </summary>
        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        /// <summary>
        /// Get entities by specification with eager loading and complex filtering/sorting/pagination
        /// </summary>
        public async Task<IEnumerable<T>> GetBySpecificationAsync(Specification<T> specification)
        {
            var query = _dbSet.AsQueryable();

            // Apply includes for eager loading
            query = specification.Includes.Aggregate(query, (current, include) => include(current));

            // Apply filtering
            if (specification.FilterExpression != null)
                query = specification.FilterExpression(query);

            // Apply sorting
            foreach (var (propertyName, isAscending) in specification.OrderByExpressions)
            {
                query = ApplyOrderBy(query, propertyName, isAscending);
            }

            // Apply pagination
            if (specification.IsPagingEnabled)
            {
                if (specification.Skip.HasValue)
                    query = query.Skip(specification.Skip.Value);

                if (specification.Take.HasValue)
                    query = query.Take(specification.Take.Value);
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// Get count of entities by specification
        /// </summary>
        public async Task<int> GetCountBySpecificationAsync(Specification<T> specification)
        {
            var query = _dbSet.AsQueryable();

            // Apply filtering (includes not needed for count)
            if (specification.FilterExpression != null)
                query = specification.FilterExpression(query);

            return await query.CountAsync();
        }

        /// <summary>
        /// Check if entity exists matching predicate
        /// </summary>
        public async Task<bool> AnyAsync(Func<T, bool> predicate)
        {
            return await Task.FromResult(_dbSet.ToList().Any(predicate));
        }

        /// <summary>
        /// Get first or default by specification
        /// </summary>
        public async Task<T?> GetFirstOrDefaultAsync(Specification<T> specification)
        {
            var query = _dbSet.AsQueryable();

            // Apply includes for eager loading
            query = specification.Includes.Aggregate(query, (current, include) => include(current));

            // Apply filtering
            if (specification.FilterExpression != null)
                query = specification.FilterExpression(query);

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Create entity
        /// </summary>
        public async Task<T> CreateAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        /// <summary>
        /// Delete entity by ID
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var existingEntity = await GetByIdAsync(id);
            if (existingEntity is null)
            {
                return false;
            }

            _dbSet.Remove(existingEntity);
            return true;
        }

        /// <summary>
        /// Update entity
        /// </summary>
        public async Task<T?> UpdateAsync(int id, T entity)
        {
            var existingEntity = await GetByIdAsync(id);
            if (existingEntity is null)
            {
                return null;
            }

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return existingEntity;
        }

        /// <summary>
        /// Create multiple entities at once
        /// </summary>
        public async Task<IEnumerable<T>> CreateRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            return entities;
        }

        /// <summary>
        /// Apply sorting by property name using reflection
        /// </summary>
        private static IQueryable<T> ApplyOrderBy(IQueryable<T> query, string propertyName, bool isAscending)
        {
            var property = typeof(T).GetProperty(propertyName, 
                System.Reflection.BindingFlags.IgnoreCase | 
                System.Reflection.BindingFlags.Public);

            if (property == null)
                return query;

            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
            var propertyAccess = System.Linq.Expressions.Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = System.Linq.Expressions.Expression.Lambda(propertyAccess, parameter);

            var methodName = isAscending ? "OrderBy" : "OrderByDescending";
            var resultExpression = System.Linq.Expressions.Expression.Call(
                typeof(System.Linq.Queryable),
                methodName,
                new[] { typeof(T), property.PropertyType },
                query.Expression,
                System.Linq.Expressions.Expression.Quote(orderByExpression));

            return query.Provider.CreateQuery<T>(resultExpression);
        }
    }
}
        