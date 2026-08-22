using FluentValidation;
using NotificationService.DTOs;

namespace NotificationService.Validators
{
    public class NotificationQueryDtoValidator : AbstractValidator<NotificationQueryDto>
    {
        public NotificationQueryDtoValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100);
        }
    }
}
