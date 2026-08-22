using FluentValidation;
using ReportService.DTOs;

namespace ReportService.Validators
{
    public class AttendanceReportRequestDtoValidator : AbstractValidator<AttendanceReportRequestDto>
    {
        public AttendanceReportRequestDtoValidator()
        {
            RuleFor(x => x)
                .Must(x => !x.DateFrom.HasValue || !x.DateTo.HasValue || x.DateFrom.Value <= x.DateTo.Value)
                .WithMessage("DateFrom must be before or equal to DateTo.");
        }
    }
}
