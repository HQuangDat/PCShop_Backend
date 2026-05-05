using FluentValidation;
using PCShop_Backend.Dtos.ProductDtos.CreateDto;

namespace PCShop_Backend.Validators.Product;

public class CreateComponentDtoValidator : AbstractValidator<createComponentDto>
{
    public CreateComponentDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Brand).MaximumLength(100).When(x => x.Brand != null);
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0.");
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ImageUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Invalid image URL format.")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description != null);
    }
}
