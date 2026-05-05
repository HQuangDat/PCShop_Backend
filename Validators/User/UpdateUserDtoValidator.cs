using FluentValidation;
using PCShop_Backend.Dtos.UserDtos.UpdateDto;

namespace PCShop_Backend.Validators.User;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.FullName).Length(1, 100).When(x => x.FullName != null);
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[\d\s\-()\[\]]+$").WithMessage("Invalid phone number format.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
        RuleFor(x => x.Address).MaximumLength(255).When(x => x.Address != null);
        RuleFor(x => x.City).MaximumLength(100).When(x => x.City != null);
        RuleFor(x => x.Country).MaximumLength(100).When(x => x.Country != null);
    }
}
