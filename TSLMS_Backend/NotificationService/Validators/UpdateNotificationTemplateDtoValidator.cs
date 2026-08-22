using FluentValidation;
using NotificationService.DTOs;

namespace NotificationService.Validators
{
    public class UpdateNotificationTemplateDtoValidator : AbstractValidator<UpdateNotificationTemplateDto>
    {
        public UpdateNotificationTemplateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.SubjectTemplate)
                .NotEmpty()
                .MaximumLength(500);

            RuleFor(x => x.BodyTemplate)
                .NotEmpty()
                .MaximumLength(8000);

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}
