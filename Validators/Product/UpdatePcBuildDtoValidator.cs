using FluentValidation;
using PCShop_Backend.Dtos.ProductDtos.UpdateDto;
using PCShop_Backend.Validators.Product;

namespace PCShop_Backend.Validators.Product;

public class UpdatePcBuildDtoValidator : AbstractValidator<UpdatePcBuildDto>
{
    public UpdatePcBuildDtoValidator()
    {
        RuleFor(x => x.BuildName).NotEmpty().Length(1, 100);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description != null);
        RuleForEach(x => x.Components)
            .SetValidator(new CreatePcBuildComponentDtoValidator())
            .When(x => x.Components != null && x.Components.Count > 0);
    }
}
