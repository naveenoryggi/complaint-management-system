using ComplaintManagement.Application.Features.Complaints.Commands;
using FluentValidation;

namespace ComplaintManagement.Application.Features.Complaints.Validators;

public class EscalateComplaintCommandValidator : AbstractValidator<EscalateComplaintCommand>
{
    public EscalateComplaintCommandValidator()
    {
        RuleFor(x => x.ComplaintId)
            .NotEmpty().WithMessage("Complaint ID is required");

        RuleFor(x => x.EscalatedById)
            .NotEmpty().WithMessage("Escalator ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Escalation reason is required")
            .MaximumLength(1000).WithMessage("Reason must not exceed 1000 characters");
    }
}
