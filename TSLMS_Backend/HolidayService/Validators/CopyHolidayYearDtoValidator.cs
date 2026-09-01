using FluentValidation;
using HolidayService.DTOs;

namespace HolidayService.Validators
{
    public class CopyHolidayYearDtoValidator : AbstractValidator<CopyHolidayYearDto>
    {
        public CopyHolidayYearDtoValidator()
        {
            RuleFor(x => x.SourceYear)
                .InclusiveBetween(2000, 2100);

            RuleFor(x => x.TargetYear)
                .InclusiveBetween(2000, 2100)
                .NotEqual(x => x.SourceYear)
                .WithMessage("Target year must be different from source year.");
        }
    }
}
