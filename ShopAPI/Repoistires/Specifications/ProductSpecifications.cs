using ShopAPI.Models;
using ShopAPI.Repoistires.Base;

namespace ShopAPI.Repoistires.Specifications
{
    /// <summary>
    /// Specification for product queries with eager loading and filtering
    /// </summary>
    public class ProductSpecification : Specification<Product>
    {
        public ProductSpecification()
        {
            // Always include Category to avoid N+1
            AddInclude(q => q.Include(p => p.Category));
        }

        /// <summary>
        /// Get all products with category
        /// </summary>
        public static ProductSpecification GetAll() => new();

        /// <summary>
        /// Get products filtered by category
        /// </summary>
        public static ProductSpecification GetByCategory(int categoryId)
        {
            var spec = new ProductSpecification();
            spec.FilterExpression = q => q.Where(p => p.CategoryId == categoryId);
            return spec;
        }

        /// <summary>
        /// Search products by name
        /// </summary>
        public static ProductSpecification Search(string searchTerm)
        {
            var spec = new ProductSpecification();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                spec.FilterExpression = q => q.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }
            return spec;
        }

        /// <summary>
        /// Filter products by price range
        /// </summary>
        public static ProductSpecification FilterByPrice(decimal minPrice, decimal maxPrice)
        {
            var spec = new ProductSpecification();
            spec.FilterExpression = q => q.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
            return spec;
        }

        /// <summary>
        /// Get products with complex filtering
        /// </summary>
        public static ProductSpecification FilterAdvanced(decimal? minPrice = null, decimal? maxPrice = null, 
            int? categoryId = null, string? searchTerm = null, string? sortBy = "name", string? sortOrder = "asc", 
            int pageNumber = 1, int pageSize = 10)
        {
            var spec = new ProductSpecification();

            // Apply filters
            spec.FilterExpression = q =>
            {
                if (minPrice.HasValue)
                    q = q.Where(p => p.Price >= minPrice.Value);

                if (maxPrice.HasValue)
                    q = q.Where(p => p.Price <= maxPrice.Value);

                if (categoryId.HasValue && categoryId.Value > 0)
                    q = q.Where(p => p.CategoryId == categoryId.Value);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                    q = q.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

                return q;
            };

            // Apply sorting
            if (sortBy != null)
            {
                var isAscending = sortOrder?.ToLower() != "desc";
                switch (sortBy.ToLower())
                {
                    case "price":
                        spec.AddOrderBy(nameof(Product.Price), isAscending);
                        break;
                    case "name":
                    default:
                        spec.AddOrderBy(nameof(Product.Name), isAscending);
                        break;
                }
            }

            // Apply pagination
            spec.IsPagingEnabled = true;
            spec.Skip = (pageNumber - 1) * pageSize;
            spec.Take = pageSize;

            return spec;
        }
    }

    /// <summary>
    /// Specification for user queries
    /// </summary>
    public class UserSpecification : Specification<User>
    {
        /// <summary>
        /// Get all users
        /// </summary>
        public static UserSpecification GetAll() => new();

        /// <summary>
        /// Get user by email (for login and duplicate checks)
        /// </summary>
        public static UserSpecification GetByEmail(string email)
        {
            var spec = new UserSpecification();
            spec.FilterExpression = q => q.Where(u => u.Email == email);
            return spec;
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        public static UserSpecification GetByUsername(string username)
        {
            var spec = new UserSpecification();
            spec.FilterExpression = q => q.Where(u => u.Username == username);
            return spec;
        }
    }
}
