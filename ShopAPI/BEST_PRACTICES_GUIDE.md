# ShopAPI - Remaining Refactoring Tasks & Best Practices

## **Quick Start - Apply These Patterns to Remaining Services**

### **Pattern 1: Service Refactoring Template**

```csharp
namespace ShopAPI.Services
{
    /// <summary>
    /// Service for managing [Entity] operations
    /// Optimized with eager loading and server-side filtering
    /// </summary>
    public class [Entity]Service : I[Entity]Service
    {
        private readonly IMainRepository<[Entity]> _mainRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<[Entity]Service> _logger; // ADD LOGGING

        public [Entity]Service(
            IMainRepository<[Entity]> mainRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<[Entity]Service> logger) // ADD LOGGER
        {
            _mainRepository = mainRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Always use specifications for queries (NO GetAllAsync() with in-memory filtering)
        /// </summary>
        public async Task<IEnumerable<[Entity]ViewDto>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all [entities]");
            
            var specification = [Entity]Specification.GetAll();
            var entities = await _mainRepository.GetBySpecificationAsync(specification);
            
            return _mapper.Map<IEnumerable<[Entity]ViewDto>>(entities);
        }

        /// <summary>
        /// ANTI-PATTERN: Never do this
        /// </summary>
        // ❌ DON'T DO THIS:
        // var all = await _mainRepository.GetAllAsync();
        // var filtered = all.Where(x => x.Property == value).ToList();
        
        // ✅ DO THIS INSTEAD:
        // var spec = new [Entity]Specification { FilterExpression = q => q.Where(...) };
        // var filtered = await _mainRepository.GetBySpecificationAsync(spec);
    }
}
```

---

## **Pattern 2: Create Specifications for Each Entity**

### **For Categories:**
```csharp
public class CategorySpecification : Specification<Category>
{
    public CategorySpecification()
    {
        // Include related data
        AddInclude(q => q.Include(c => c.Products));
    }

    public static CategorySpecification GetAll() => new();

    public static CategorySpecification GetById(int id)
    {
        var spec = new CategorySpecification();
        spec.FilterExpression = q => q.Where(c => c.Id == id);
        return spec;
    }

    public static CategorySpecification WithPagination(int pageNumber, int pageSize)
    {
        var spec = new CategorySpecification();
        spec.IsPagingEnabled = true;
        spec.Skip = (pageNumber - 1) * pageSize;
        spec.Take = pageSize;
        return spec;
    }
}
```

### **For Orders:**
```csharp
public class OrderSpecification : Specification<Order>
{
    public OrderSpecification()
    {
        // Eager load related entities
        AddInclude(q => q.Include(o => o.User));
        AddInclude(q => q.Include(o => o.OrderItems).ThenInclude(oi => oi.Product));
        AddInclude(q => q.Include(o => o.Addresses));
    }

    public static OrderSpecification GetByUserId(int userId)
    {
        var spec = new OrderSpecification();
        spec.FilterExpression = q => q.Where(o => o.UserId == userId);
        spec.AddOrderBy(nameof(Order.CreatedDate), false); // Newest first
        return spec;
    }

    public static OrderSpecification GetById(int id)
    {
        var spec = new OrderSpecification();
        spec.FilterExpression = q => q.Where(o => o.Id == id);
        return spec;
    }
}
```

---

## **Pattern 3: Controller Refactoring Template**

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class [Entities]Controller : ControllerBase
{
    private readonly I[Entity]Service _service;
    private readonly IValidator<[Entity]CreateDto> _createValidator;
    private readonly IValidator<[Entity]UpdateDto> _updateValidator;
    private readonly ILogger<[Entities]Controller> _logger;

    public [Entities]Controller(
        I[Entity]Service service,
        IValidator<[Entity]CreateDto> createValidator,
        IValidator<[Entity]UpdateDto> updateValidator,
        ILogger<[Entities]Controller> logger)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<[Entity]ViewDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<[Entity]ViewDto>>>> GetAllAsync()
    {
        _logger.LogInformation("Getting all [entities]");
        var entities = await _service.GetAllAsync();
        
        return Ok(ApiResponse<IEnumerable<[Entity]ViewDto>>.SuccessResponse(
            entities,
            $"Retrieved {entities.Count()} [entities]"
        ));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<[Entity]ViewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<[Entity]ViewDto>>> GetByIdAsync(int id)
    {
        _logger.LogInformation($"Getting [entity] with ID: {id}");
        var entity = await _service.GetByIdAsync(id);
        
        return Ok(ApiResponse<[Entity]ViewDto>.SuccessResponse(
            entity,
            "[Entity] retrieved successfully"
        ));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<[Entity]ViewDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<[Entity]ViewDto>>> CreateAsync([Entity]CreateDto dto)
    {
        _logger.LogInformation("Creating new [entity]");

        // Validate input
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var entity = await _service.CreateAsync(dto);
        
        return CreatedAtAction(nameof(GetByIdAsync), new { id = entity.Id },
            ApiResponse<[Entity]ViewDto>.SuccessResponse(
                entity,
                "[Entity] created successfully",
                201
            ));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<[Entity]ViewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<[Entity]ViewDto>>> UpdateAsync(
        int id,
        [Entity]UpdateDto dto)
    {
        _logger.LogInformation($"Updating [entity] with ID: {id}");

        // Validate input
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var entity = await _service.UpdateAsync(id, dto);
        
        return Ok(ApiResponse<[Entity]ViewDto>.SuccessResponse(
            entity,
            "[Entity] updated successfully"
        ));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        _logger.LogInformation($"Deleting [entity] with ID: {id}");
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
```

---

## **Refactoring Checklist for Each Controller**

### **Step 1: Create Validators**
- [ ] Create `[Entity]Validators.cs` in `Validators/` folder
- [ ] Add CreateDto validator
- [ ] Add UpdateDto validator (if needed)
- [ ] Add specific business rule validation

### **Step 2: Create Specifications**
- [ ] Create `[Entity]Specifications.cs` in `Repoistires/Specifications/`
- [ ] Implement GetAll() with eager loading
- [ ] Implement GetById() spec
- [ ] Implement custom filter specs
- [ ] Implement pagination specs

### **Step 3: Refactor Service**
- [ ] Add ILogger parameter
- [ ] Replace GetAllAsync() calls with specifications
- [ ] Remove all in-memory filtering (LINQ .Where on materialized data)
- [ ] Add logging for key operations
- [ ] Use GetFirstOrDefaultAsync for single-record queries
- [ ] Use GetCountBySpecificationAsync for count operations

### **Step 4: Refactor Controller**
- [ ] Remove ApiControllerBase inheritance
- [ ] Add validators to constructor
- [ ] Add logger to constructor
- [ ] Remove try-catch blocks (handled by middleware)
- [ ] Add FluentValidation calls in POST/PUT
- [ ] Use ApiResponse<T> for all responses
- [ ] Add ProducesResponseType attributes
- [ ] Add XML documentation
- [ ] Update route to `/api/v1/[controller]`
- [ ] Remove HandleException calls

---

## **Services Needing Refactoring**

```
Priority Order:
1. ✅ ProductService - DONE
2. ✅ AccountService - DONE
3. [ ] CategoryService - Similar pattern
4. [ ] OrderService - Complex (has related entities)
5. [ ] OrderItemService - Simpler CRUD
6. [ ] CartService - Related to multiple entities
7. [ ] CartItemService - Related to cart
8. [ ] UserService - Simple CRUD
9. [ ] AddressService - Simple CRUD
```

---

## **Common Mistakes to Avoid**

### ❌ **Mistake 1: Loading All Data**
```csharp
// BAD
var items = await repo.GetAllAsync();
return items.Where(x => x.Status == "Active").ToList();

// GOOD
var spec = new Specification();
spec.FilterExpression = q => q.Where(x => x.Status == "Active");
return await repo.GetBySpecificationAsync(spec);
```

### ❌ **Mistake 2: Missing Eager Loading**
```csharp
// BAD - N+1 query problem
var orders = await repo.GetAllAsync();
foreach(var order in orders)
{
    var user = order.User; // Extra query per order!
}

// GOOD
var spec = new OrderSpecification(); // Always includes User
var orders = await repo.GetBySpecificationAsync(spec);
foreach(var order in orders)
{
    var user = order.User; // Already loaded
}
```

### ❌ **Mistake 3: Empty Validation**
```csharp
// BAD
public async Task<ProductViewDto> CreateProductAsync(ProductCreateDto dto)
{
    var product = _mapper.Map<Product>(dto);
    // No validation!
}

// GOOD
public async Task<ProductViewDto> CreateProductAsync(ProductCreateDto dto)
{
    var result = await _validator.ValidateAsync(dto);
    if (!result.IsValid)
        throw new ValidationException(result.Errors);
    
    var product = _mapper.Map<Product>(dto);
}
```

### ❌ **Mistake 4: Try-Catch in Controllers**
```csharp
// BAD - Duplicated across controllers
[HttpGet("{id}")]
public async Task<ActionResult> Get(int id)
{
    try
    {
        var item = await _service.GetAsync(id);
        return Ok(item);
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}

// GOOD - Middleware handles it
[HttpGet("{id}")]
public async Task<ActionResult> Get(int id)
{
    var item = await _service.GetAsync(id);
    return Ok(ApiResponse<Item>.SuccessResponse(item));
}
```

---

## **Adding New Features Using Patterns**

### **Feature: Add Search to Categories**

**Step 1: Update Specification**
```csharp
public class CategorySpecification : Specification<Category>
{
    public static CategorySpecification Search(string searchTerm)
    {
        var spec = new CategorySpecification();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            spec.FilterExpression = q => q.Where(c => 
                c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }
        return spec;
    }
}
```

**Step 2: Add Service Method**
```csharp
public async Task<IEnumerable<CategoryViewDto>> SearchAsync(string searchTerm)
{
    _logger.LogInformation($"Searching categories: {searchTerm}");
    var spec = CategorySpecification.Search(searchTerm);
    var categories = await _mainRepository.GetBySpecificationAsync(spec);
    return _mapper.Map<IEnumerable<CategoryViewDto>>(categories);
}
```

**Step 3: Add Controller Endpoint**
```csharp
[HttpGet("search")]
public async Task<ActionResult<ApiResponse<IEnumerable<CategoryViewDto>>>> SearchAsync(
    [FromQuery] string searchTerm)
{
    var categories = await _service.SearchAsync(searchTerm);
    return Ok(ApiResponse<IEnumerable<CategoryViewDto>>.SuccessResponse(categories));
}
```

---

## **Performance Tips**

### **1. Always Use Specifications for Queries**
```csharp
// Every query should specify what to include
var spec = new ProductSpecification();
spec.FilterExpression = q => q.Where(p => p.Price > 100);
var products = await _repo.GetBySpecificationAsync(spec);
```

### **2. Use Pagination for List Endpoints**
```csharp
var spec = ProductSpecification.FilterAdvanced(
    pageNumber: 1,
    pageSize: 20  // Never return unlimited results
);
var page = await _repo.GetBySpecificationAsync(spec);
```

### **3. Implement Caching for Frequently Accessed Data**
```csharp
public async Task<CategoryViewDto> GetCategoryByIdAsync(int id)
{
    const string cacheKey = $"category_{id}";
    if (!_cache.TryGetValue(cacheKey, out CategoryViewDto? cached))
    {
        var spec = new CategorySpecification { FilterExpression = q => q.Where(c => c.Id == id) };
        var category = await _repo.GetFirstOrDefaultAsync(spec);
        _cache.Set(cacheKey, category, TimeSpan.FromHours(1));
        return _mapper.Map<CategoryViewDto>(category);
    }
    return cached!;
}
```

### **4. Use Select() for Limited Fields**
```csharp
// Instead of loading entire objects, load only needed fields
var spec = new ProductSpecification();
spec.FilterExpression = q => q
    .Select(p => new { p.Id, p.Name, p.Price })
    .Where(p => p.Price > 100);
```

---

## **Database Migrations**

### **Create migration for any schema changes:**
```bash
cd "d:\Web API Projects\ShopAPI\ShopAPI"
dotnet ef migrations add [MigrationName]
dotnet ef database update
```

### **Example migrations needed:**
- Add indexes on frequently queried columns
- Add constraints for data integrity
- Optimize foreign key relationships

---

## **Testing Strategy**

### **Unit Tests for Services**
```csharp
[TestClass]
public class ProductServiceTests
{
    [TestMethod]
    public async Task GetProductsByCategory_ShouldReturnOnlySelectedCategory()
    {
        // Arrange
        var mockRepo = new Mock<IMainRepository<Product>>();
        var service = new ProductService(mockRepo.Object, mockUnitOfWork, mockMapper, mockLogger);

        // Act
        await service.GetProductsByCategoryAsync(1);

        // Assert
        mockRepo.Verify(
            r => r.GetBySpecificationAsync(It.IsAny<ProductSpecification>()),
            Times.Once);
    }
}
```

### **Integration Tests for Endpoints**
```csharp
[TestClass]
public class ProductsControllerTests
{
    [TestMethod]
    public async Task GetProducts_ReturnsOkWithPaginatedData()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/products?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsAsync<ApiResponse<PagedResult<ProductViewDto>>>();
        content.Success.Should().BeTrue();
        content.Data.Items.Should().NotBeEmpty();
    }
}
```

---

## **Deployment Checklist**

- [ ] All services refactored with specifications
- [ ] All controllers use response wrappers
- [ ] FluentValidation integrated everywhere
- [ ] Logging added throughout
- [ ] Tests passing (unit + integration)
- [ ] Performance tested with realistic data
- [ ] Security review completed
- [ ] HTTPS enforced
- [ ] CORS configured for production
- [ ] Application Insights configured
- [ ] Database backed up
- [ ] Deployment guide documented

---

## **Monitoring & Maintenance**

### **Key Metrics to Monitor:**
- API response times
- Database query performance
- Error rates and types
- User activity
- Cache hit rates
- Memory usage

### **Logging Strategy:**
- INFO: Key operations (login, create, delete)
- WARNING: Potential issues (duplicate email, etc.)
- ERROR: Failures requiring attention
- DEBUG: Detailed flow (development only)

---

## **Questions? Follow This Framework**

When adding new features or refactoring:

1. **Does it use specifications?** ✅
2. **Does it validate input?** ✅
3. **Does it have logging?** ✅
4. **Does it use response wrappers?** ✅
5. **Is error handling centralized?** ✅
6. **Are relationships eager-loaded?** ✅
7. **Is it tested?** ✅

If yes to all, you're ready to deploy! 🚀
