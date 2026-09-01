using FluentValidation;
using LeaveService.DTOs;

namespace LeaveService.Validators
{
    public class ApproveLeaveDtoValidator : AbstractValidator<ApproveLeaveDto>
    {
        public ApproveLeaveDtoValidator()
        {
            RuleFor(x => x.Comment)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.Comment));
        }
    }
}
