using FluentValidation;
using LeaveService.DTOs;

namespace LeaveService.Validators
{
    public class UpdateLeaveTypeDtoValidator : AbstractValidator<UpdateLeaveTypeDto>
    {
        public UpdateLeaveTypeDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Description).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Description));
            RuleFor(x => x.DefaultAnnualQuota).GreaterThanOrEqualTo(0);
            RuleFor(x => x.MaxCarryForwardDays).GreaterThanOrEqualTo(0);
        }
    }
}
