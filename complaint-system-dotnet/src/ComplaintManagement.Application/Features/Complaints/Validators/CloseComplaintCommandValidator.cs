using ComplaintManagement.Application.Features.Complaints.Commands;
using FluentValidation;

namespace ComplaintManagement.Application.Features.Complaints.Validators;

public class CloseComplaintCommandValidator : AbstractValidator<CloseComplaintCommand>
{
    public CloseComplaintCommandValidator()
    {
        RuleFor(x => x.ComplaintId)
            .NotEmpty().WithMessage("Complaint ID is required");

        RuleFor(x => x.ClosedById)
            .NotEmpty().WithMessage("Closer ID is required");

        RuleFor(x => x.ResolutionNotes)
            .NotEmpty().WithMessage("Resolution notes are required")
            .MaximumLength(4000).WithMessage("Resolution notes must not exceed 4000 characters");
    }
}
