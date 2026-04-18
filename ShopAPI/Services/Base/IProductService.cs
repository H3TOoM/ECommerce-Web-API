using ShopAPI.Common.Pagination;
using ShopAPI.DTOs;

namespace ShopAPI.Services.Base
{
    /// <summary>
    /// Service interface for product operations
    /// </summary>
    public interface IProductService
    {
        Task<IEnumerable<ProductViewDto>> GetAllProductsAsync();
        Task<IEnumerable<ProductViewDto>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<ProductViewDto>> SearchProductsAsync(string searchTerm);        
        Task<IEnumerable<ProductViewDto>> FilterByPrice(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<ProductViewDto>> SortByPrice(decimal price);
        Task<ProductViewDto> GetProductByIdAsync(int productId);
        Task<ProductViewDto> CreateProductAsync(ProductCreateDto dto);
        Task<ProductViewDto> UpdateProductAsync(int id, ProductUpdateDto dto);
        Task<bool> DeleteProductAsync(int id);
        
        /// <summary>
        /// Advanced filtering, sorting, and pagination - NEW
        /// </summary>
        Task<PagedResult<ProductViewDto>> GetProductsAdvancedAsync(ProductFilterParams filterParams);
    }
}
