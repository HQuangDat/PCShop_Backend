using FluentValidation;
using PCShop_Backend.Dtos.CartDtos.UpdateDtos;

namespace PCShop_Backend.Validators.Cart;

public class UpdateCartItemsDtoValidator : AbstractValidator<UpdateCartItemsDto>
{
    public UpdateCartItemsDtoValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
