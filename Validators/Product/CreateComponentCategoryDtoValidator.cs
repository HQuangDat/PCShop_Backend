using FluentValidation;
using PCShop_Backend.Dtos.ProductDtos.CreateDto;

namespace PCShop_Backend.Validators.Product;

public class CreateComponentCategoryDtoValidator : AbstractValidator<CreateComponentCategoryDto>
{
    public CreateComponentCategoryDtoValidator()
    {
        RuleFor(x => x.CategoryName).NotEmpty().Length(1, 50);
        RuleFor(x => x.Description).MaximumLength(255).When(x => x.Description != null);
    }
}
