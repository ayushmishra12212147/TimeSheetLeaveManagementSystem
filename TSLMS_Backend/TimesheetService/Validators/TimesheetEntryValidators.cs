using FluentValidation;
using TimesheetService.DTOs;

namespace TimesheetService.Validators
{
    public class CreateTimesheetEntryDtoValidator : AbstractValidator<CreateTimesheetEntryDto>
    {
        public CreateTimesheetEntryDtoValidator()
        {
            RuleFor(x => x.EntryDate).Must(x => x != default).WithMessage("Entry date is required.");
            RuleFor(x => x.ProjectId).NotEmpty();
            RuleFor(x => x.Hours).InclusiveBetween(0.5m, 24m);
            RuleFor(x => x.Description).MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }

    public class UpdateTimesheetEntryDtoValidator : AbstractValidator<UpdateTimesheetEntryDto>
    {
        public UpdateTimesheetEntryDtoValidator()
        {
            RuleFor(x => x.EntryDate).Must(x => x != default).WithMessage("Entry date is required.");
            RuleFor(x => x.ProjectId).NotEmpty();
            RuleFor(x => x.Hours).InclusiveBetween(0.5m, 24m);
            RuleFor(x => x.Description).MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }

    public class SubmitTimesheetDtoValidator : AbstractValidator<SubmitTimesheetDto>
    {
        public SubmitTimesheetDtoValidator()
        {
            RuleFor(x => x.WeekStartDate)
                .Must(x => !x.HasValue || x.Value != default)
                .WithMessage("Week start date is invalid.");
        }
    }

    public class ApproveTimesheetDtoValidator : AbstractValidator<ApproveTimesheetDto>
    {
        public ApproveTimesheetDtoValidator()
        {
            RuleFor(x => x.Comment).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Comment));
        }
    }

    public class RejectTimesheetDtoValidator : AbstractValidator<RejectTimesheetDto>
    {
        public RejectTimesheetDtoValidator()
        {
            RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
        }
    }
}
