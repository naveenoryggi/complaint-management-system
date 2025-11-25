using ComplaintManagement.Application.Features.Complaints.Commands;
using FluentValidation;

namespace ComplaintManagement.Application.Features.Complaints.Validators;

public class AssignComplaintCommandValidator : AbstractValidator<AssignComplaintCommand>
{
    public AssignComplaintCommandValidator()
    {
        RuleFor(x => x.ComplaintId)
            .NotEmpty().WithMessage("Complaint ID is required");

        RuleFor(x => x.AssignedToId)
            .NotEmpty().WithMessage("User ID to assign is required");

        RuleFor(x => x.AssignedById)
            .NotEmpty().WithMessage("Assigner ID is required");
    }
}
