using FluentValidation;
using PCShop_Backend.Dtos.UserDtos.CreateDto;

namespace PCShop_Backend.Validators.User;

public class CreateRoleDtoValidator : AbstractValidator<CreateRoleDto>
{
    public CreateRoleDtoValidator()
    {
        RuleFor(x => x.RoleName).NotEmpty().Length(1, 100);
        RuleFor(x => x.Description).MaximumLength(255).When(x => x.Description != null);
    }
}
