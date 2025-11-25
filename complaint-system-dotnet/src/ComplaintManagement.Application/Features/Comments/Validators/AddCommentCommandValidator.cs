using ComplaintManagement.Application.Features.Comments.Commands;
using FluentValidation;

namespace ComplaintManagement.Application.Features.Comments.Validators;

public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.ComplaintId)
            .NotEmpty().WithMessage("Complaint ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required")
            .MaximumLength(2000).WithMessage("Comment must not exceed 2000 characters");
    }
}
