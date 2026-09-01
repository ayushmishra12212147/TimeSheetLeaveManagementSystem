using FluentValidation;
using ReportService.DTOs;
using ReportService.Enums;

namespace ReportService.Validators
{
    public class CreateReportRequestDtoValidator : AbstractValidator<CreateReportRequestDto>
    {
        public CreateReportRequestDtoValidator()
        {
            RuleFor(x => x.ReportType)
                .Must(x => x is ReportType.Leave or ReportType.Timesheet or ReportType.Attendance)
                .WithMessage("Only leave, timesheet, and attendance reports support approval workflow.");

            RuleFor(x => x)
                .Must(x => !x.DateFrom.HasValue || !x.DateTo.HasValue || x.DateFrom.Value <= x.DateTo.Value)
                .WithMessage("DateFrom must be before or equal to DateTo.");
        }
    }

    public class RejectReportRequestDtoValidator : AbstractValidator<RejectReportRequestDto>
    {
        public RejectReportRequestDtoValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty()
                .MaximumLength(1000);
        }
    }
}
