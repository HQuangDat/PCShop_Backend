using FluentValidation;
using PCShop_Backend.Dtos.ProductDtos.CreateDto;

namespace PCShop_Backend.Validators.Product;

public class CreateComponentSpecDtoValidator : AbstractValidator<CreateComponentSpecDto>
{
    public CreateComponentSpecDtoValidator()
    {
        RuleFor(x => x.ComponentId).GreaterThan(0);
        RuleFor(x => x.SpecKey).NotEmpty().Length(1, 50);
        RuleFor(x => x.SpecValue).NotEmpty().Length(1, 255);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0).When(x => x.DisplayOrder.HasValue);
    }
}
