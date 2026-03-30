using AutoMapper;
using ShopAPI.DTOs;
using ShopAPI.Helpers;
using ShopAPI.Models;
using ShopAPI.Repoistires.Base;
using ShopAPI.Services.Base;

namespace ShopAPI.Services
{
    /// <summary>
    /// Service for managing user account operations including registration and authentication
    /// </summary>
    public class AccountService : IAccountService
    {
        private readonly IMainRepository<User> _mainRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        public AccountService(
            IMainRepository<User> mainRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ITokenService tokenService)
        {
            _mainRepository = mainRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        public async Task<UserViewDto> RegisterAsync(UserCreateDto dto)
        {
            if (dto.IsNullEntity())
                throw new ArgumentNullException(nameof(dto));

            var users = await _mainRepository.GetAllAsync();
            var existingUser = users.FirstOrDefault(u => u.Email == dto.Email);
            if (existingUser != null)
                throw new ArgumentException("User with this email already exists");

            var user = _mapper.Map<User>(dto);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _mainRepository.CreateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserViewDto>(user);
        }

        public async Task<AuthResponseDto> LoginAsync(UserLoginDto dto)
        {
            if (dto.IsNullEntity())
                throw new ArgumentNullException(nameof(dto));

            // Find user by email and password
            var users = await _mainRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email == dto.Email);

            if (user.IsNotFound() || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new ArgumentException("Invalid email or password");

            var tokenResult = _tokenService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = tokenResult.Token,
                ExpiresAt = tokenResult.ExpiresAt,
                User = _mapper.Map<UserViewDto>(user)
            };
        }

        #endregion
    }
}


