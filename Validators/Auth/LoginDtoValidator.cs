using FluentValidation;
using PCShop_Backend.Dtos.AuthDtos;

namespace PCShop_Backend.Validators.Auth;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.username).NotEmpty().MaximumLength(50);
        RuleFor(x => x.password).NotEmpty().MinimumLength(6);
    }
}
