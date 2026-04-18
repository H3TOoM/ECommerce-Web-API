using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using ShopAPI.Common.Pagination;
using ShopAPI.Common.Responses;
using ShopAPI.DTOs;
using ShopAPI.Services.Base;

namespace ShopAPI.Controllers;

/// <summary>
/// Products API endpoints
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IValidator<ProductCreateDto> _createValidator;
    private readonly IValidator<ProductUpdateDto> _updateValidator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        IValidator<ProductCreateDto> createValidator,
        IValidator<ProductUpdateDto> updateValidator,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    /// <summary>
    /// Get all products with pagination and advanced filtering
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 100)</param>
    /// <param name="sortBy">Sort by field: 'name' or 'price'</param>
    /// <param name="sortOrder">Sort order: 'asc' or 'desc'</param>
    /// <param name="searchTerm">Search term for product name</param>
    /// <param name="minPrice">Minimum price filter</param>
    /// <param name="maxPrice">Maximum price filter</param>
    /// <param name="categoryId">Category ID filter</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductViewDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductViewDto>>>> GetAllAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortOrder = "asc",
        [FromQuery] string? searchTerm = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int? categoryId = null)
    {
        _logger.LogInformation("Getting products with pagination");

        var filterParams = new ProductFilterParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortOrder = sortOrder,
            SearchTerm = searchTerm,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            CategoryId = categoryId
        };

        var products = await _productService.GetProductsAdvancedAsync(filterParams);
        return Ok(ApiResponse<PagedResult<ProductViewDto>>.SuccessResponse(
            products,
            $"Retrieved {products.Items.Count()} products",
            200
        ));
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductViewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductViewDto>>> GetByIdAsync(int id)
    {
        _logger.LogInformation($"Getting product with ID: {id}");

        var product = await _productService.GetProductByIdAsync(id);
        return Ok(ApiResponse<ProductViewDto>.SuccessResponse(product, "Product retrieved successfully"));
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    [HttpGet("category/{categoryId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductViewDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductViewDto>>>> GetByCategoryAsync(int categoryId)
    {
        _logger.LogInformation($"Getting products for category: {categoryId}");

        var products = await _productService.GetProductsByCategoryAsync(categoryId);
        return Ok(ApiResponse<IEnumerable<ProductViewDto>>.SuccessResponse(
            products,
            $"Retrieved {products.Count()} products"
        ));
    }

    /// <summary>
    /// Search products by name
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductViewDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductViewDto>>>> SearchAsync([FromQuery] string term)
    {
        _logger.LogInformation($"Searching products with term: {term}");

        var products = await _productService.SearchProductsAsync(term);
        return Ok(ApiResponse<IEnumerable<ProductViewDto>>.SuccessResponse(
            products,
            $"Found {products.Count()} products"
        ));
    }

    /// <summary>
    /// Filter products by price range
    /// </summary>
    [HttpGet("filter")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductViewDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductViewDto>>>> FilterByPriceAsync(
        [FromQuery] decimal minPrice,
        [FromQuery] decimal maxPrice)
    {
        _logger.LogInformation($"Filtering products by price: {minPrice} - {maxPrice}");

        if (minPrice > maxPrice)
            throw new ArgumentException("Minimum price cannot exceed maximum price");

        var products = await _productService.FilterByPrice(minPrice, maxPrice);
        return Ok(ApiResponse<IEnumerable<ProductViewDto>>.SuccessResponse(
            products,
            $"Found {products.Count()} products in price range"
        ));
    }

    /// <summary>
    /// Create new product
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductViewDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProductViewDto>>> CreateAsync(ProductCreateDto dto)
    {
        _logger.LogInformation($"Creating new product: {dto.Name}");

        // Validate input
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());
            
            throw new ValidationException(validationResult.Errors);
        }

        var product = await _productService.CreateProductAsync(dto);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = product.Id },
            ApiResponse<ProductViewDto>.SuccessResponse(product, "Product created successfully", 201));
    }

    /// <summary>
    /// Update existing product
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductViewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProductViewDto>>> UpdateAsync(int id, ProductUpdateDto dto)
    {
        _logger.LogInformation($"Updating product with ID: {id}");

        // Validate input
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var product = await _productService.UpdateProductAsync(id, dto);
        return Ok(ApiResponse<ProductViewDto>.SuccessResponse(product, "Product updated successfully"));
    }

    /// <summary>
    /// Delete product
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        _logger.LogInformation($"Deleting product with ID: {id}");

        var deleted = await _productService.DeleteProductAsync(id);
        if (!deleted)
            throw new KeyNotFoundException($"Product with ID {id} not found");

        return NoContent();
    }
}

