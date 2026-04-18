using AutoMapper;
using ShopAPI.DTOs;
using ShopAPI.Helpers;
using ShopAPI.Models;
using ShopAPI.Repoistires.Base;
using ShopAPI.Services.Base;

namespace ShopAPI.Services
{
    /// <summary>
    /// Service for managing cart item operations including CRUD functionality
    /// </summary>
    public class CartItemService : ICartItemService
    {
        private readonly IMainRepository<CartItem> _mainRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        
        public CartItemService(IMainRepository<CartItem> mainRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mainRepository = mainRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #region CRUD Operations

        /// <summary>
        /// Creates a new cart item
        /// </summary>
        /// <param name="cartId">Cart ID to add item to</param>
        /// <param name="dto">Cart item creation data transfer object</param>
        /// <returns>Created cart item view DTO</returns>
        /// <exception cref="ArgumentNullException">Thrown when DTO is null</exception>
        /// <exception cref="ArgumentException">Thrown when cart ID is invalid</exception>
        public async Task<CartItemViewDto> CreateCartItemAsync(int cartId, CartItemCreateDto dto)
        {

            if (cartId.IsInvalidId())
                throw new ArgumentException("Invalid Cart ID!");

            // Map DTO to entity and set cart ID
            var cartItem = _mapper.Map<CartItem>(dto);
            cartItem.CartId = cartId;

            await _mainRepository.CreateAsync(cartItem);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CartItemViewDto>(cartItem);
        }

        /// <summary>
        /// Gets all cart items for a specific cart
        /// </summary>
        /// <param name="cartId">Cart ID</param>
        /// <returns>List of cart items in the cart</returns>
        /// <exception cref="ArgumentException">Thrown when cart ID is invalid</exception>
        public async Task<IEnumerable<CartItemViewDto>> GetAllCartItemsAsync(int cartId)
        {
            if (cartId.IsInvalidId())
                throw new ArgumentException("Invalid Cart ID!");

            // Filter cart items by cart ID
            var cartItems = await _mainRepository.GetAllAsync();
            var filteredItems = cartItems
                .Where(ci => ci.CartId == cartId)
                .ToList();

            return _mapper.Map<IEnumerable<CartItemViewDto>>(filteredItems);
        }

        /// <summary>
        /// Gets a cart item by ID
        /// </summary>
        /// <param name="id">Cart item ID</param>
        /// <returns>Cart item view DTO</returns>
        /// <exception cref="ArgumentException">Thrown when ID is invalid or cart item not found</exception>
        public async Task<CartItemViewDto> GetCartItemByIdAsync(int id)
        {
            if (id.IsInvalidId())
                throw new ArgumentException("Invalid ID!");

            var cartItem = await _mainRepository.GetByIdAsync(id);
            if (cartItem.IsNotFound())
                throw new ArgumentException("Cart Item Not Found");

            return _mapper.Map<CartItemViewDto>(cartItem);
        }

        /// <summary>
        /// Updates an existing cart item
        /// </summary>
        /// <param name="id">Cart item ID to update</param>
        /// <param name="dto">Cart item update data transfer object</param>
        /// <returns>Updated cart item view DTO</returns>
        /// <exception cref="ArgumentException">Thrown when ID is invalid or cart item not found</exception>
        /// <exception cref="ArgumentNullException">Thrown when DTO is null</exception>
        public async Task<CartItemViewDto> UpdateCartItemAsync(int id, CartItemUpdateDto dto)
        {
            if (id.IsInvalidId())
                throw new ArgumentException("Invalid ID!");

            if (dto.IsNullEntity())
                throw new ArgumentNullException(nameof(dto));

            var cartItem = await _mainRepository.GetByIdAsync(id);
            if (cartItem.IsNotFound())
                throw new ArgumentException("Cart Item Not Found");

            // Update cart item properties
            cartItem.ProductId = dto.ProductId;
            cartItem.Quantity = dto.Quantity;

            await _mainRepository.UpdateAsync(id, cartItem);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CartItemViewDto>(cartItem);
        }

        /// <summary>
        /// Deletes a cart item by ID
        /// </summary>
        /// <param name="id">Cart item ID to delete</param>
        /// <returns>True if deletion was successful, false if cart item not found</returns>
        /// <exception cref="ArgumentException">Thrown when ID is invalid</exception>
        public async Task<bool> DeleteCartItemAsync(int id)
        {
            if (id.IsInvalidId())
                throw new ArgumentException("Invalid ID!");

            var cartItem = await _mainRepository.GetByIdAsync(id);
            if (cartItem.IsNotFound())
                return false;

            await _mainRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        #endregion
    }
}

