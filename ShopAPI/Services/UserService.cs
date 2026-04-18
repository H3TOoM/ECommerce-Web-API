using AutoMapper;
using ShopAPI.DTOs;
using ShopAPI.Helpers;
using ShopAPI.Models;
using ShopAPI.Repoistires.Base;
using ShopAPI.Services.Base;

namespace ShopAPI.Services
{
    /// <summary>
    /// Service for managing user operations including CRUD functionality
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IMainRepository<User> _mainRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        
        public UserService(IMainRepository<User> mainRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mainRepository = mainRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="dto">User creation data transfer object</param>
        /// <returns>Created user view DTO</returns>
        /// <exception cref="ArgumentNullException">Thrown when DTO is null</exception>
        /// <remarks>Password should be hashed in production environment</remarks>
        public async Task<UserViewDto> CreateUserAsync(UserCreateDto dto)
        {
            if (dto.IsNullEntity())
                throw new ArgumentNullException(nameof(dto));

            // Map DTO to entity and set password hash
            var user = _mapper.Map<User>(dto);
            // Note: Password should be hashed in production
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _mainRepository.CreateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserViewDto>(user);
        }

        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns>List of all users</returns>
        public async Task<IEnumerable<UserViewDto>> GetAllUsersAsync()
        {
            var users = await _mainRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserViewDto>>(users);
        }

        /// <summary>
        /// Gets a user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User view DTO</returns>
        /// <exception cref="ArgumentException">Thrown when ID is invalid or user not found</exception>
        public async Task<UserViewDto> GetUserByIdAsync(int id)
        {
            if (id.IsInvalidId())
                throw new ArgumentException("Invalid ID!");

            var user = await _mainRepository.GetByIdAsync(id);
            if (user.IsNotFound())
                throw new ArgumentException("User Not Found");

            return _mapper.Map<UserViewDto>(user);
        }

        /// <summary>
        /// Updates an existing user
        /// </summary>
        /// <param name="id">User ID to update</param>
        /// <param name="dto">User update data transfer object</param>
        /// <returns>Updated user view DTO</returns>
        /// <exception cref="ArgumentException">Thrown when ID is invalid or user not found</exception>
        /// <exception cref="ArgumentNullException">Thrown when DTO is null</exception>
        /// <remarks>Password should be hashed in production environment</remarks>
        public async Task<UserViewDto> UpdateUserAsync(int id, UserUpdateDto dto)
        {
            if (id.IsInvalidId())
                throw new ArgumentException("Invalid ID!");


            var user = await _mainRepository.GetByIdAsync(id);
            if (user.IsNotFound())
                throw new ArgumentException("User Not Found");

            // Update only provided fields (null-coalescing for optional updates)
            user.Username = dto.Username ?? user.Username;
            user.Email = dto.Email ?? user.Email;
            if (!string.IsNullOrEmpty(dto.Password))
                user.PasswordHash = dto.Password; // Note: In production, this should be hashed
            user.Role = dto.Role ?? user.Role;

            await _mainRepository.UpdateAsync(id, user);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserViewDto>(user);
        }

        /// <summary>
        /// Deletes a user by ID
        /// </summary>
        /// <param name="id">User ID to delete</param>
        /// <returns>True if deletion was successful, false if user not found</returns>
        /// <exception cref="ArgumentException">Thrown when ID is invalid</exception>
        public async Task<bool> DeleteUserAsync(int id)
        {
            if (id.IsInvalidId())
                throw new ArgumentException("Invalid ID!");

            var user = await _mainRepository.GetByIdAsync(id);
            if (user.IsNotFound())
                return false;

            await _mainRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

    }
}

