using FluentValidation;
using PCShop_Backend.Dtos.CartDtos.CreateDtos;

namespace PCShop_Backend.Validators.Cart;

public class AddItemToCartDtoValidator : AbstractValidator<AddItemToCartDtos>
{
    public AddItemToCartDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => x.ComponentId.HasValue || x.BuildId.HasValue)
            .WithMessage("Either ComponentId or BuildId must be provided.");
        RuleFor(x => x.ComponentId).GreaterThan(0).When(x => x.ComponentId.HasValue);
        RuleFor(x => x.BuildId).GreaterThan(0).When(x => x.BuildId.HasValue);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
