using FluentValidation;
using LeaveService.DTOs;
using LeaveService.Enums;

namespace LeaveService.Validators
{
    public class UpdateLeaveRequestDtoValidator : AbstractValidator<UpdateLeaveRequestDto>
    {
        public UpdateLeaveRequestDtoValidator()
        {
            RuleFor(x => x.LeaveTypeId).NotEmpty();
            RuleFor(x => x.StartDate).Must(x => x != default).WithMessage("Start date is required.");
            RuleFor(x => x.EndDate)
                .Must((dto, endDate) => endDate >= dto.StartDate)
                .WithMessage("End date must be on or after start date.");
            RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.SupportingDocumentUrl).MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.SupportingDocumentUrl));

            When(x => x.IsHalfDay, () =>
            {
                RuleFor(x => x.StartDate)
                    .Equal(x => x.EndDate)
                    .WithMessage("Half-day leave must be for a single date.");

                RuleFor(x => x.HalfDaySession)
                    .NotEqual(HalfDaySession.None)
                    .WithMessage("Half-day session is required when applying for half-day leave.");
            });

            When(x => !x.IsHalfDay, () =>
            {
                RuleFor(x => x.HalfDaySession)
                    .Equal(HalfDaySession.None)
                    .WithMessage("Half-day session should be None for full-day leave.");
            });
        }
    }
}
