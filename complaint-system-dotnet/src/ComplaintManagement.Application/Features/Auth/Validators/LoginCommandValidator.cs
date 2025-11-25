using ComplaintManagement.Application.Features.Auth.Commands;
using FluentValidation;

namespace ComplaintManagement.Application.Features.Auth.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UserIdentifier)
            .NotEmpty().WithMessage("User identifier (Email, Employee Code, or Phone) is required")
            .MaximumLength(100).WithMessage("User identifier cannot exceed 100 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(3).WithMessage("Password must be at least 3 characters");
    }
}
