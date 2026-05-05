using FluentValidation;
using PCShop_Backend.Dtos.UserDtos.UpdateDto;

namespace PCShop_Backend.Validators.User;

public class UpdateRoleDtoValidator : AbstractValidator<UpdateRoleDto>
{
    public UpdateRoleDtoValidator()
    {
        RuleFor(x => x.RoleName).NotEmpty().Length(1, 100);
        RuleFor(x => x.Description).MaximumLength(255).When(x => x.Description != null);
    }
}
