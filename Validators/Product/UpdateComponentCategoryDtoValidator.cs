using FluentValidation;
using PCShop_Backend.Dtos.ProductDtos.UpdateDto;

namespace PCShop_Backend.Validators.Product;

public class UpdateComponentCategoryDtoValidator : AbstractValidator<UpdateComponentCategoryDto>
{
    public UpdateComponentCategoryDtoValidator()
    {
        RuleFor(x => x.CategoryName).NotEmpty().Length(1, 50);
        RuleFor(x => x.Description).MaximumLength(255).When(x => x.Description != null);
    }
}
