using FluentValidation;
using ShopAPI.DTOs;

namespace ShopAPI.Validators
{
    /// <summary>
    /// Validators for User and Authentication DTOs
    /// </summary>
    public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
    {
        public UserCreateDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MaximumLength(50).WithMessage("Username cannot exceed 50 characters")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters")
                .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("Username can only contain letters, numbers, underscores, and hyphens");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be valid")
                .MaximumLength(150).WithMessage("Email cannot exceed 150 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .Matches(@"^(?=.*[A-Za-z])(?=.*\d)")
                .WithMessage("Password must contain at least one letter and one number");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required")
                .MaximumLength(50).WithMessage("Role cannot exceed 50 characters")
                .Must(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
                           r.Equals("Customer", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Role must be either 'Admin' or 'Customer'");
        }
    }

    public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
    {
        public UserUpdateDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MaximumLength(50).WithMessage("Username cannot exceed 50 characters")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be valid")
                .MaximumLength(150).WithMessage("Email cannot exceed 150 characters");

            RuleFor(x => x.Password)
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .When(x => !string.IsNullOrEmpty(x.Password))
                .Matches(@"^(?=.*[A-Za-z])(?=.*\d)")
                .WithMessage("Password must contain at least one letter and one number")
                .When(x => !string.IsNullOrEmpty(x.Password));

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required")
                .MaximumLength(50).WithMessage("Role cannot exceed 50 characters")
                .Must(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
                           r.Equals("Customer", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Role must be either 'Admin' or 'Customer'");
        }
    }

    public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
    {
        public UserLoginDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be valid");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }
}
