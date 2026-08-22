using FluentValidation;
using HolidayService.DTOs;

namespace HolidayService.Validators
{
    public class CreateHolidayDtoValidator : AbstractValidator<CreateHolidayDto>
    {
        public CreateHolidayDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.HolidayDate)
                .Must(x => x != default)
                .WithMessage("Holiday date is required.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}
