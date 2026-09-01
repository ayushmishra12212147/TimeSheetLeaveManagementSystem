using FluentValidation;
using LeaveService.DTOs;

namespace LeaveService.Validators
{
    public class AdjustBalanceDtoValidator : AbstractValidator<AdjustBalanceDto>
    {
        public AdjustBalanceDtoValidator()
        {
            RuleFor(x => x.Days).NotEqual(0m);
            RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        }
    }
}
