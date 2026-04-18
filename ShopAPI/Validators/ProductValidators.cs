using FluentValidation;
using ShopAPI.DTOs;

namespace ShopAPI.Validators
{
    /// <summary>
    /// Validators for Product DTOs
    /// </summary>
    public class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
    {
        public ProductCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters")
                .MinimumLength(3).WithMessage("Product name must be at least 3 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0")
                .LessThan(decimal.MaxValue).WithMessage("Price exceeds maximum allowed value");

            RuleFor(x => x.ImageUrl)
                .Must(x => string.IsNullOrEmpty(x) || Uri.IsWellFormedUriString(x, UriKind.Absolute))
                .WithMessage("Image URL must be a valid absolute URL");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("A valid category must be selected");
        }
    }

    public class ProductUpdateDtoValidator : AbstractValidator<ProductUpdateDto>
    {
        public ProductUpdateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters")
                .MinimumLength(3).WithMessage("Product name must be at least 3 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0")
                .LessThan(decimal.MaxValue).WithMessage("Price exceeds maximum allowed value");

            RuleFor(x => x.ImageUrl)
                .Must(x => string.IsNullOrEmpty(x) || Uri.IsWellFormedUriString(x, UriKind.Absolute))
                .WithMessage("Image URL must be a valid absolute URL");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("A valid category must be selected");
        }
    }
}
