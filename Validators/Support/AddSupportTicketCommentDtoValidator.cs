using FluentValidation;
using PCShop_Backend.Dtos.SupportDtos.CreateDtos;

namespace PCShop_Backend.Validators.Support;

public class AddSupportTicketCommentDtoValidator : AbstractValidator<AddSupportTicketCommentDto>
{
    public AddSupportTicketCommentDtoValidator()
    {
        RuleFor(x => x.CommentText).NotEmpty().MaximumLength(2000);
    }
}
