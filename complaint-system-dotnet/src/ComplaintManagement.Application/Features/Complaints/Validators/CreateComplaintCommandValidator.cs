using ComplaintManagement.Application.Features.Complaints.Commands;
using FluentValidation;

namespace ComplaintManagement.Application.Features.Complaints.Validators;

public class CreateComplaintCommandValidator : AbstractValidator<CreateComplaintCommand>
{
    public CreateComplaintCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(500).WithMessage("Title must not exceed 500 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(4000).WithMessage("Description must not exceed 4000 characters");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value");

        RuleFor(x => x.ComplainantId)
            .NotEmpty().WithMessage("Complainant ID is required");

        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Company ID is required");
    }
}
