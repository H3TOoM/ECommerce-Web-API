using AutoMapper;
using ShopAPI.DTOs;
using ShopAPI.Helpers;
using ShopAPI.Models;
using ShopAPI.Repoistires.Base;
using ShopAPI.Repoistires.Specifications;
using ShopAPI.Services.Base;

namespace ShopAPI.Services
{
    /// <summary>
    /// Service for managing user account operations including registration and authentication
    /// Optimized to avoid N+1 queries and inefficient lookups
    /// </summary>
    public class AccountService : IAccountService
    {
        private readonly IMainRepository<User> _mainRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IMainRepository<User> mainRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ITokenService tokenService,
            ILogger<AccountService> logger)
        {
            _mainRepository = mainRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user - OPTIMIZED: uses specification to check email existence
        /// </summary>
        public async Task<UserViewDto> RegisterAsync(UserCreateDto dto)
        {
            if (dto.IsNullEntity())
                throw new ArgumentNullException(nameof(dto));

            // Use specification pattern to check if user already exists (single query, no loading all users)
            var existingUser = await _mainRepository.GetFirstOrDefaultAsync(
                UserSpecification.GetByEmail(dto.Email));

            if (existingUser != null)
            {
                _logger.LogWarning($"Registration attempt with existing email: {dto.Email}");
                throw new ArgumentException("User with this email already exists");
            }

            var user = _mapper.Map<User>(dto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _mainRepository.CreateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"User registered: {user.Email}");
            return _mapper.Map<UserViewDto>(user);
        }

        /// <summary>
        /// Login user and return JWT token - OPTIMIZED: uses specification pattern
        /// </summary>
        public async Task<AuthResponseDto> LoginAsync(UserLoginDto dto)
        {
            if (dto.IsNullEntity())
                throw new ArgumentNullException(nameof(dto));

            // Use specification to find user by email (single query, not loading all users)
            var user = await _mainRepository.GetFirstOrDefaultAsync(
                UserSpecification.GetByEmail(dto.Email));

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning($"Failed login attempt for email: {dto.Email}");
                throw new ArgumentException("Invalid email or password");
            }

            var tokenResult = _tokenService.GenerateToken(user);

            _logger.LogInformation($"User logged in: {user.Email}");

            return new AuthResponseDto
            {
                Token = tokenResult.Token,
                ExpiresAt = tokenResult.ExpiresAt,
                User = _mapper.Map<UserViewDto>(user)
            };
        }
    }
}


