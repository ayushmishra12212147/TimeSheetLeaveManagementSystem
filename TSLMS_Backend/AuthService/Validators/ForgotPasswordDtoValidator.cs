using AuthService.DTOs;
using FluentValidation;

namespace AuthService.Validators
{
    public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
    {
        public ForgotPasswordDtoValidator()
        {
            RuleFor(x => x.EmployeeId)
                .NotEmpty().WithMessage("Employee ID is required.")
                .Matches("^(EMP|MGR|ADM)\\d{5}$").WithMessage("Employee ID format is invalid.");
        }
    }
}
