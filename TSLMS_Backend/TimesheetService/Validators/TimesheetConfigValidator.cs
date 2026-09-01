using FluentValidation;
using TimesheetService.DTOs;

namespace TimesheetService.Validators
{
    public class UpdateTimesheetConfigDtoValidator : AbstractValidator<UpdateTimesheetConfigDto>
    {
        public UpdateTimesheetConfigDtoValidator()
        {
            RuleFor(x => x.MinimumWeeklyHours).InclusiveBetween(1m, 168m);
            RuleFor(x => x.LowHoursWarningThreshold).InclusiveBetween(0.5m, 24m);
            RuleFor(x => x.HighHoursWarningThreshold).InclusiveBetween(0.5m, 24m);
            RuleFor(x => x.HighHoursWarningThreshold)
                .GreaterThanOrEqualTo(x => x.LowHoursWarningThreshold)
                .WithMessage("High-hours threshold must be greater than or equal to low-hours threshold.");
            RuleFor(x => x.AutoApproveAfterHours).InclusiveBetween(1, 720);
        }
    }
}
