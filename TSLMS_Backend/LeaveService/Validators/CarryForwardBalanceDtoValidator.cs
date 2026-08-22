using FluentValidation;
using LeaveService.DTOs;

namespace LeaveService.Validators
{
    public class CarryForwardBalanceDtoValidator : AbstractValidator<CarryForwardBalanceDto>
    {
        public CarryForwardBalanceDtoValidator()
        {
            RuleFor(x => x.SourceYear).InclusiveBetween(2000, 2100);
            RuleFor(x => x.TargetYear)
                .InclusiveBetween(2000, 2100)
                .NotEqual(x => x.SourceYear)
                .WithMessage("Target year must be different from source year.");
        }
    }
}
