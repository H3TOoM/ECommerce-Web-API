using AutoMapper;
using ShopAPI.DTOs;
using ShopAPI.Helpers;
using ShopAPI.Models;
using ShopAPI.Repoistires.Base;
using ShopAPI.Services.Base;

namespace ShopAPI.Services
{
    /// <summary>
    /// Service for managing product operations including CRUD and search/filter functionality
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IMainRepository<Product> _mainRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        
        public ProductService(IMainRepository<Product> mainRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mainRepository = mainRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ProductViewDto> CreateProductAsync(ProductCreateDto dto)
        {
            if (dto.IsNullEntity())
                throw new ArgumentNullException(nameof(dto));

            var product = _mapper.Map<Product>(dto);
            await _mainRepository.CreateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductViewDto>(product);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            if (id.IsInvalidId())
                throw new ArgumentException("Invalid ID!");

            var product = await _mainRepository.GetByIdAsync(id);
            if (product.IsNotFound())
                return false;

            await _mainRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ProductViewDto>> GetAllProductsAsync()
        {
            var products = await _mainRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductViewDto>>(products);
        }

        public async Task<ProductViewDto> GetProductByIdAsync(int productId)
        {
            if (productId.IsInvalidId())
                throw new ArgumentException("Invalid ID!");

            var product = await _mainRepository.GetByIdAsync(productId);
            if (product.IsNotFound())
                throw new ArgumentException("Product Not Found");

            return _mapper.Map<ProductViewDto>(product);
        }

        public async Task<ProductViewDto> UpdateProductAsync(int id, ProductUpdateDto dto)
        {
            if (id.IsInvalidId())
                throw new ArgumentException("Invalid ID!");


            var product = await _mainRepository.GetByIdAsync(id);
            if (product.IsNotFound())
                throw new ArgumentException("Product Not Found");

            product.Name = dto.Name ?? product.Name;
            if (dto.Price != 0)
                product.Price = dto.Price;
            product.ImageUrl = dto.ImageUrl ?? product.ImageUrl;
            if (dto.CategoryId != 0)
                product.CategoryId = dto.CategoryId;

            await _mainRepository.UpdateAsync(id, product);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductViewDto>(product);
        }

        public async Task<IEnumerable<ProductViewDto>> FilterByPrice(decimal minPrice, decimal maxPrice)
        {
            var products = await _mainRepository.GetAllAsync();
            var filteredProducts = products
                .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
                .ToList();

            return _mapper.Map<IEnumerable<ProductViewDto>>(filteredProducts);
        }

        public async Task<IEnumerable<ProductViewDto>> GetProductsByCategoryAsync(int categoryId)
        {
            if (categoryId.IsInvalidId())
                throw new ArgumentException("Invalid Category ID!");

            var products = await _mainRepository.GetAllAsync();
            var filteredProducts = products
                .Where(p => p.CategoryId == categoryId)
                .ToList();

            return _mapper.Map<IEnumerable<ProductViewDto>>(filteredProducts);
        }

        public async Task<IEnumerable<ProductViewDto>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Search term cannot be empty");

            var products = await _mainRepository.GetAllAsync();
            var searchedProducts = products
                .Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return _mapper.Map<IEnumerable<ProductViewDto>>(searchedProducts);
        }

        public async Task<IEnumerable<ProductViewDto>> SortByPrice(decimal price)
        {
            var products = await _mainRepository.GetAllAsync();
            var sortedProducts = products
                .OrderByDescending(p => p.Price)
                .ToList();

            return _mapper.Map<IEnumerable<ProductViewDto>>(sortedProducts);
        }

        #endregion
    }
}
