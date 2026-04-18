using Microsoft.EntityFrameworkCore;

namespace ShopAPI.Repoistires.Base
{
    /// <summary>
    /// Specification pattern for building complex queries with filtering, sorting, and including related entities
    /// </summary>
    public abstract class Specification<T> where T : class
    {
        /// <summary>
        /// LINQ expression for filtering
        /// </summary>
        public Func<IQueryable<T>, IQueryable<T>>? FilterExpression { get; set; }

        /// <summary>
        /// Navigation properties to include in the query
        /// </summary>
        public List<Func<IQueryable<T>, IQueryable<T>>> Includes { get; } = new();

        /// <summary>
        /// Add an include for eager loading
        /// </summary>
        protected virtual void AddInclude(Func<IQueryable<T>, IQueryable<T>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        /// <summary>
        /// LINQ expressions for sorting (propertyName, isAscending)
        /// </summary>
        public List<(string PropertyName, bool IsAscending)> OrderByExpressions { get; } = new();

        /// <summary>
        /// Add a sorting expression
        /// </summary>
        protected virtual void AddOrderBy(string propertyName, bool isAscending = true)
        {
            OrderByExpressions.Add((propertyName, isAscending));
        }

        /// <summary>
        /// Take (limit) clause
        /// </summary>
        public int? Take { get; set; }

        /// <summary>
        /// Skip (offset) clause
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// Is pagination enabled
        /// </summary>
        public bool IsPagingEnabled { get; set; }
    }
}
