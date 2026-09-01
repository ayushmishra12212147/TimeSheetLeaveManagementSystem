using FluentValidation;
using LeaveService.DTOs;

namespace LeaveService.Validators
{
    public class RejectLeaveDtoValidator : AbstractValidator<RejectLeaveDto>
    {
        public RejectLeaveDtoValidator()
        {
            RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
        }
    }
}
