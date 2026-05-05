using FluentValidation;
using PCShop_Backend.Dtos.SupportDtos.UpdateDtos;

namespace PCShop_Backend.Validators.Support;

public class UpdateSupportTicketCommentDtoValidator : AbstractValidator<UpdateSupportTicketCommentDto>
{
    public UpdateSupportTicketCommentDtoValidator()
    {
        RuleFor(x => x.CommentText).NotEmpty().MaximumLength(2000);
    }
}
