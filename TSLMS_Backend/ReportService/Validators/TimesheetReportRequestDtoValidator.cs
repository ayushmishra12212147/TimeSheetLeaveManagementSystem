using FluentValidation;
using ReportService.DTOs;

namespace ReportService.Validators
{
    public class TimesheetReportRequestDtoValidator : AbstractValidator<TimesheetReportRequestDto>
    {
        public TimesheetReportRequestDtoValidator()
        {
            RuleFor(x => x)
                .Must(x => !x.DateFrom.HasValue || !x.DateTo.HasValue || x.DateFrom.Value <= x.DateTo.Value)
                .WithMessage("DateFrom must be before or equal to DateTo.");

            RuleFor(x => x)
                .Must(x => !x.DateFrom.HasValue || !x.DateTo.HasValue || x.DateTo.Value.DayNumber - x.DateFrom.Value.DayNumber <= 366)
                .WithMessage("Timesheet report range cannot exceed 12 months.");
        }
    }
}
