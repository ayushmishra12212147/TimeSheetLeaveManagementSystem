using FluentValidation;
using ReportService.DTOs;

namespace ReportService.Validators
{
    public class ExportRequestDtoValidator : AbstractValidator<ExportRequestDto>
    {
        private static readonly string[] AllowedFormats = ["excel", "pdf"];

        public ExportRequestDtoValidator()
        {
            RuleFor(x => x.Format)
                .NotEmpty()
                .Must(x => AllowedFormats.Contains(x.Trim(), StringComparer.OrdinalIgnoreCase))
                .WithMessage("Format must be either 'excel' or 'pdf'.");
        }
    }
}
