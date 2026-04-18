using AutoMapper;
using ShopAPI.Common.Pagination;
using ShopAPI.DTOs;
using ShopAPI.Helpers;
using ShopAPI.Models;
using ShopAPI.Repoistires.Base;
using ShopAPI.Repoistires.Specifications;
using ShopAPI.Services.Base;

namespace ShopAPI.Services
{
    /// <summary>
    /// Service for managing product operations including CRUD and search/filter functionality
    /// Optimized with eager loading and server-side filtering to avoid N+1 queries
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IMainRepository<Product> _mainRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IMainRepository<Product> mainRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ProductService> logger)
        {
            _mainRepository = mainRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        public async Task<ProductViewDto> CreateProductAsync(ProductCreateDto dto)
        {
            if (dto.IsNullEntity())
                throw new ArgumentNullException(nameof(dto));

            var product = _mapper.Map<Product>(dto);
            await _mainRepository.CreateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Product created with ID: {product.Id}");
            return _mapper.Map<ProductViewDto>(product);
        }

        /// <summary>
        /// Delete product by ID
        /// </summary>
        public async Task<bool> DeleteProductAsync(int id)
        {
            if (id.IsInvalidId())
                throw new ArgumentException("Invalid ID!");

            var product = await _mainRepository.GetByIdAsync(id);
            if (product.IsNotFound())
                return false;

            await _mainRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Product deleted with ID: {id}");
            return true;
        }

        /// <summary>
        /// Get all products with pagination (default: page 1, 10 items per page)
        /// </summary>
        public async Task<IEnumerable<ProductViewDto>> GetAllProductsAsync()
        {
            _logger.LogInformation("Fetching all products");
            var specification = ProductSpecification.FilterAdvanced(pageNumber: 1, pageSize: 100);
            var products = await _mainRepository.GetBySpecificationAsync(specification);
            return _mapper.Map<IEnumerable<ProductViewDto>>(products);
        }

        /// <summary>
        /// Get product by ID with category eager-loaded
        /// </summary>
        public async Task<ProductViewDto> GetProductByIdAsync(int productId)
        {
            if (productId.IsInvalidId())
                throw new ArgumentException("Invalid ID!");

            var product = await _mainRepository.GetByIdAsync(productId);
            if (product.IsNotFound())
                throw new KeyNotFoundException($"Product with ID {productId} not found");

            return _mapper.Map<ProductViewDto>(product);
        }

        /// <summary>
        /// Update product by ID
        /// </summary>
        public async Task<ProductViewDto> UpdateProductAsync(int id, ProductUpdateDto dto)
        {
            if (id.IsInvalidId())
                throw new ArgumentException("Invalid ID!");

            var product = await _mainRepository.GetByIdAsync(id);
            if (product.IsNotFound())
                throw new KeyNotFoundException($"Product with ID {id} not found");

            product.Name = dto.Name ?? product.Name;
            if (dto.Price != 0)
                product.Price = dto.Price;
            product.ImageUrl = dto.ImageUrl ?? product.ImageUrl;
            if (dto.CategoryId != 0)
                product.CategoryId = dto.CategoryId;

            await _mainRepository.UpdateAsync(id, product);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Product updated with ID: {id}");
            return _mapper.Map<ProductViewDto>(product);
        }

        /// <summary>
        /// Filter products by price range (server-side query - NO N+1)
        /// </summary>
        public async Task<IEnumerable<ProductViewDto>> FilterByPrice(decimal minPrice, decimal maxPrice)
        {
            _logger.LogInformation($"Filtering products by price range: {minPrice} - {maxPrice}");
            var specification = ProductSpecification.FilterByPrice(minPrice, maxPrice);
            var products = await _mainRepository.GetBySpecificationAsync(specification);
            return _mapper.Map<IEnumerable<ProductViewDto>>(products);
        }

        /// <summary>
        /// Get products by category (server-side query - NO N+1)
        /// </summary>
        public async Task<IEnumerable<ProductViewDto>> GetProductsByCategoryAsync(int categoryId)
        {
            if (categoryId.IsInvalidId())
                throw new ArgumentException("Invalid Category ID!");

            _logger.LogInformation($"Fetching products for category ID: {categoryId}");
            var specification = ProductSpecification.GetByCategory(categoryId);
            var products = await _mainRepository.GetBySpecificationAsync(specification);
            return _mapper.Map<IEnumerable<ProductViewDto>>(products);
        }

        /// <summary>
        /// Search products by name (server-side query - NO N+1)
        /// </summary>
        public async Task<IEnumerable<ProductViewDto>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Search term cannot be empty");

            _logger.LogInformation($"Searching products with term: '{searchTerm}'");
            var specification = ProductSpecification.Search(searchTerm);
            var products = await _mainRepository.GetBySpecificationAsync(specification);
            return _mapper.Map<IEnumerable<ProductViewDto>>(products);
        }

        /// <summary>
        /// Sort products by price (server-side query - NO N+1)
        /// </summary>
        public async Task<IEnumerable<ProductViewDto>> SortByPrice(decimal price)
        {
            _logger.LogInformation("Sorting products by price");
            var specification = new ProductSpecification();
            specification.AddOrderBy(nameof(Product.Price), false); // descending
            var products = await _mainRepository.GetBySpecificationAsync(specification);
            return _mapper.Map<IEnumerable<ProductViewDto>>(products);
        }

        /// <summary>
        /// Advanced product filtering with pagination, sorting, and search
        /// </summary>
        public async Task<PagedResult<ProductViewDto>> GetProductsAdvancedAsync(ProductFilterParams filterParams)
        {
            _logger.LogInformation($"Advanced product filtering: page {filterParams.PageNumber}, " +
                $"pageSize {filterParams.PageSize}, sortBy {filterParams.SortBy}");

            var specification = ProductSpecification.FilterAdvanced(
                minPrice: filterParams.MinPrice,
                maxPrice: filterParams.MaxPrice,
                categoryId: filterParams.CategoryId,
                searchTerm: filterParams.SearchTerm,
                sortBy: filterParams.SortBy,
                sortOrder: filterParams.SortOrder,
                pageNumber: filterParams.PageNumber,
                pageSize: filterParams.PageSize
            );

            var totalCount = await _mainRepository.GetCountBySpecificationAsync(
                new ProductSpecification { FilterExpression = specification.FilterExpression }
            );
            var products = await _mainRepository.GetBySpecificationAsync(specification);

            var dtos = _mapper.Map<IEnumerable<ProductViewDto>>(products);
            return PagedResult<ProductViewDto>.Create(dtos, totalCount, filterParams.PageNumber, filterParams.PageSize);
        }
    }
}

