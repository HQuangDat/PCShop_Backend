using FluentValidation;
using PCShop_Backend.Dtos.UserDtos.CreateDto;

namespace PCShop_Backend.Validators.User;

public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserDtoValidator()
    {
        RuleFor(x => x.Username).NotEmpty().Length(3, 50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(100);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[\d\s\-()\[\]]+$").WithMessage("Invalid phone number format.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
        RuleFor(x => x.Address).MaximumLength(255).When(x => x.Address != null);
        RuleFor(x => x.City).MaximumLength(100).When(x => x.City != null);
        RuleFor(x => x.Country).MaximumLength(100).When(x => x.Country != null);
    }
}
