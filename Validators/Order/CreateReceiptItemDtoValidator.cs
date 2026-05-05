using FluentValidation;
using PCShop_Backend.Dtos.OrderDtos.CreateDtos;

namespace PCShop_Backend.Validators.Order;

public class CreateReceiptItemDtoValidator : AbstractValidator<CreateReceiptItemDto>
{
    public CreateReceiptItemDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => x.ComponentId.HasValue || x.BuildId.HasValue)
            .WithMessage("Either ComponentId or BuildId must be provided.");
        RuleFor(x => x.ComponentId).GreaterThan(0).When(x => x.ComponentId.HasValue);
        RuleFor(x => x.BuildId).GreaterThan(0).When(x => x.BuildId.HasValue);
        RuleFor(x => x.ItemName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThan(0);
    }
}
