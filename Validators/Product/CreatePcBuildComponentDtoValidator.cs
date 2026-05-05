using FluentValidation;
using PCShop_Backend.Dtos.ProductDtos.CreateDto;

namespace PCShop_Backend.Validators.Product;

public class CreatePcBuildComponentDtoValidator : AbstractValidator<CreatePcBuildComponentDto>
{
    public CreatePcBuildComponentDtoValidator()
    {
        RuleFor(x => x.ComponentId).GreaterThan(0);
        RuleFor(x => x.Quantity).InclusiveBetween(1, 10);
    }
}
