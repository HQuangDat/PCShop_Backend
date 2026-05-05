using FluentValidation;
using PCShop_Backend.Dtos.OrderDtos.CreateDtos;

namespace PCShop_Backend.Validators.Order;

public class CreateReceiptDtoValidator : AbstractValidator<CreateReceiptDto>
{
    public CreateReceiptDtoValidator()
    {
        RuleFor(x => x.TotalAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PaymentMethod).MaximumLength(50).When(x => x.PaymentMethod != null);
        RuleFor(x => x.ShippingAddress).MaximumLength(255).When(x => x.ShippingAddress != null);
        RuleFor(x => x.City).MaximumLength(100).When(x => x.City != null);
        RuleFor(x => x.Country).MaximumLength(100).When(x => x.Country != null);
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => x.Notes != null);
    }
}
