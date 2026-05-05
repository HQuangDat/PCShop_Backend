using FluentValidation;
using PCShop_Backend.Dtos.UserDtos.UpdateDto;

namespace PCShop_Backend.Validators.User;

public class ChangePasswordDtoValidator : AbstractValidator<ChangePassWordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).MaximumLength(100);
    }
}
