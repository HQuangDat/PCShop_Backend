using FluentValidation;
using PCShop_Backend.Dtos.ProductDtos.CreateDto;

namespace PCShop_Backend.Validators.Product;

public class CreatePcBuildDtoValidator : AbstractValidator<CreatePcBuildDto>
{
    public CreatePcBuildDtoValidator()
    {
        RuleFor(x => x.BuildName).NotEmpty().Length(1, 100);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description != null);
        RuleFor(x => x.Components).NotEmpty().WithMessage("Build must have at least one component.");
        RuleForEach(x => x.Components).SetValidator(new CreatePcBuildComponentDtoValidator());
    }
}
