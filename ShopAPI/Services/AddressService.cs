using AutoMapper;
using ShopAPI.DTOs;
using ShopAPI.Helpers;
using ShopAPI.Models;
using ShopAPI.Repoistires.Base;
using ShopAPI.Services.Base;

namespace ShopAPI.Services
{
    /// <summary>
    /// Service for managing address operations including CRUD and user-specific queries
    /// </summary>
    public class AddressService : IAddressService
    {
        private readonly IMainRepository<Address> _mainRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        
        public AddressService(IMainRepository<Address> mainRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mainRepository = mainRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates a new address for a user
        /// </summary>
        /// <param name="userId">User ID to associate the address with</param>
        /// <param name="dto">Address creation data transfer object</param>
        /// <returns>Created address view DTO</returns>
        /// <exception cref="ArgumentNullException">Thrown when DTO is null</exception>
        /// <exception cref="ArgumentException">Thrown when user ID is invalid</exception>
        public async Task<AddressViewDto> CreateAddressAsync(int userId, AddressCreateDto dto)
        {
            if (dto.IsNullEntity())
                throw new ArgumentNullException(nameof(dto));

            if (userId.IsInvalidId())
                throw new ArgumentException("Invalid User ID!");

            // Map DTO to entity and set user ID
            var address = _mapper.Map<Address>(dto);
            address.UserId = userId;

            await _mainRepository.CreateAsync(address);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AddressViewDto>(address);
        }

        /// <summary>
        /// Gets all addresses
        /// </summary>
        /// <returns>List of all addresses</returns>
        public async Task<IEnumerable<AddressViewDto>> GetAllAddressesAsync()
        {
            var addresses = await _mainRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<AddressViewDto>>(addresses);
        }

        /// <summary>
        /// Gets an address by ID
        /// </summary>
        /// <param name="id">Address ID</param>
        /// <returns>Address view DTO</returns>
        /// <exception cref="ArgumentException">Thrown when ID is invalid or address not found</exception>
        public async Task<AddressViewDto> GetAddressByIdAsync(int id)
        {
            if (id.IsInvalidId())
                throw new ArgumentException("Invalid ID!");

            var address = await _mainRepository.GetByIdAsync(id);
            if (address.IsNotFound())
                throw new ArgumentException("Address Not Found");

            return _mapper.Map<AddressViewDto>(address);
        }

        /// <summary>
        /// Updates an existing address for a user
        /// </summary>
        /// <param name="userId">User ID to find the address</param>
        /// <param name="dto">Address update data transfer object</param>
        /// <returns>Updated address view DTO</returns>
        /// <exception cref="ArgumentException">Thrown when user ID is invalid or address not found</exception>
        /// <exception cref="ArgumentNullException">Thrown when DTO is null</exception>
        public async Task<AddressViewDto> UpdateAddressAsync(int userId, AddressUpdateDto dto)
        {
            if (userId.IsInvalidId())
                throw new ArgumentException("Invalid User ID!");


            // Find address by user ID
            var addresses = await _mainRepository.GetAllAsync();
            var address = addresses.FirstOrDefault(a => a.UserId == userId);

            if (address.IsNotFound())
                throw new ArgumentException("Address Not Found");

            // Update only provided fields (null-coalescing for optional updates)
            address.Street = dto.Street ?? address.Street;
            address.City = dto.City ?? address.City;
            address.State = dto.State ?? address.State;
            address.PostalCode = dto.PostalCode ?? address.PostalCode;
            address.Country = dto.Country ?? address.Country;

            await _mainRepository.UpdateAsync(address.Id, address);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AddressViewDto>(address);
        }

        /// <summary>
        /// Deletes an address by ID
        /// </summary>
        /// <param name="id">Address ID to delete</param>
        /// <returns>True if deletion was successful, false if address not found</returns>
        /// <exception cref="ArgumentException">Thrown when ID is invalid</exception>
        public async Task<bool> DeleteAddressAsync(int id)
        {
            if (id.IsInvalidId())
                throw new ArgumentException("Invalid ID!");

            var address = await _mainRepository.GetByIdAsync(id);
            if (address.IsNotFound())
                return false;

            await _mainRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Query Operations

        /// <summary>
        /// Gets all addresses for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of addresses belonging to the user</returns>
        /// <exception cref="ArgumentException">Thrown when user ID is invalid</exception>
        public async Task<IEnumerable<AddressViewDto>> GetAddressesByUserIdAsync(int userId)
        {
            if (userId.IsInvalidId())
                throw new ArgumentException("Invalid User ID!");

            // Filter addresses by user ID
            var addresses = await _mainRepository.GetAllAsync();
            var userAddresses = addresses.Where(a => a.UserId == userId).ToList();

            return _mapper.Map<IEnumerable<AddressViewDto>>(userAddresses);
        }

        #endregion
    }
}

