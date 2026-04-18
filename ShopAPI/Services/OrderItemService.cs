using AutoMapper;
using ShopAPI.DTOs;
using ShopAPI.Helpers;
using ShopAPI.Models;
using ShopAPI.Repoistires.Base;
using ShopAPI.Services.Base;

namespace ShopAPI.Services
{
    /// <summary>
    /// Service for managing order item operations including CRUD functionality
    /// </summary>
    public class OrderItemService : IOrderItemService
    {
        private readonly IMainRepository<OrderItem> _mainRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        
        public OrderItemService(IMainRepository<OrderItem> mainRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mainRepository = mainRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        

        #region CRUD Operations

        /// <summary>
        /// Creates a new order item
        /// </summary>
        /// <param name="orderId">Order ID to add item to</param>
        /// <param name="dto">Order item creation data transfer object</param>
        /// <returns>Created order item view DTO</returns>
        /// <exception cref="ArgumentNullException">Thrown when DTO is null</exception>
        /// <exception cref="ArgumentException">Thrown when order ID is invalid</exception>
        public async Task<OrderItemViewDto> CreateOrderItemAsync(int orderId, OrderItemCreateDto dto)
        {
            if (dto.IsNullEntity())
                throw new ArgumentNullException(nameof(dto));

            if (orderId.IsInvalidId())
                throw new ArgumentException("Invalid Order ID!");

            // Map DTO to entity and set order ID
            var orderItem = _mapper.Map<OrderItem>(dto);
            orderItem.OrderId = orderId;

            await _mainRepository.CreateAsync(orderItem);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<OrderItemViewDto>(orderItem);
        }

        /// <summary>
        /// Gets all order items for a specific order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>List of order items in the order</returns>
        /// <exception cref="ArgumentException">Thrown when order ID is invalid</exception>
        public async Task<IEnumerable<OrderItemViewDto>> GetAllOrderItemsAsync(int orderId)
        {
            if (orderId.IsInvalidId())
                throw new ArgumentException("Invalid Order ID!");

            // Filter order items by order ID
            var orderItems = await _mainRepository.GetAllAsync();
            var filteredItems = orderItems
                .Where(oi => oi.OrderId == orderId)
                .ToList();

            return _mapper.Map<IEnumerable<OrderItemViewDto>>(filteredItems);
        }

        /// <summary>
        /// Gets an order item by ID
        /// </summary>
        /// <param name="id">Order item ID</param>
        /// <returns>Order item view DTO</returns>
        /// <exception cref="ArgumentException">Thrown when ID is invalid or order item not found</exception>
        public async Task<OrderItemViewDto> GetOrderItemByIdAsync(int id)
        {
            if (id.IsInvalidId())
                throw new ArgumentException("Invalid ID!");

            var orderItem = await _mainRepository.GetByIdAsync(id);
            if (orderItem.IsNotFound())
                throw new ArgumentException("Order Item Not Found");

            return _mapper.Map<OrderItemViewDto>(orderItem);
        }

        /// <summary>
        /// Updates an existing order item
        /// </summary>
        /// <param name="id">Order item ID to update</param>
        /// <param name="dto">Order item update data transfer object</param>
        /// <returns>Updated order item view DTO</returns>
        /// <exception cref="ArgumentException">Thrown when ID is invalid or order item not found</exception>
        /// <exception cref="ArgumentNullException">Thrown when DTO is null</exception>
        public async Task<OrderItemViewDto> UpdateOrderItemAsync(int id, OrderItemUpdateDto dto)
        {
            if (id.IsInvalidId())
                throw new ArgumentException("Invalid ID!");

            var orderItem = await _mainRepository.GetByIdAsync(id);
            if (orderItem.IsNotFound())
                throw new ArgumentException("Order Item Not Found");

            // Update order item properties
            orderItem.ProductId = dto.ProductId;
            orderItem.Quantity = dto.Quantity;
            orderItem.UnitPrice = dto.UnitPrice;

            await _mainRepository.UpdateAsync(id, orderItem);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<OrderItemViewDto>(orderItem);
        }

        /// <summary>
        /// Deletes an order item by ID
        /// </summary>
        /// <param name="id">Order item ID to delete</param>
        /// <returns>True if deletion was successful, false if order item not found</returns>
        /// <exception cref="ArgumentException">Thrown when ID is invalid</exception>
        public async Task<bool> DeleteOrderItemAsync(int id)
        {
            if (id.IsInvalidId())
                throw new ArgumentException("Invalid ID!");

            var orderItem = await _mainRepository.GetByIdAsync(id);
            if (orderItem.IsNotFound())
                return false;

            await _mainRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        #endregion
    }
}

