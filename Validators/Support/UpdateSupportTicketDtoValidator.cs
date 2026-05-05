using FluentValidation;
using PCShop_Backend.Dtos.SupportDtos.UpdateDtos;

namespace PCShop_Backend.Validators.Support;

public class UpdateSupportTicketDtoValidator : AbstractValidator<UpdateSupportTicketDto>
{
    public UpdateSupportTicketDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description != null);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Priority).MaximumLength(50).When(x => x.Priority != null);
    }
}
