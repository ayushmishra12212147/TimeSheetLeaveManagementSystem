using EmployeeService.DTOs;
using FluentValidation;

namespace EmployeeService.Validators
{
    public class UserDtoValidator : AbstractValidator<UserDto>
    {
        public UserDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full Name is required");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required")
                .Must(role => role is "Employee" or "Manager" or "HRAdmin")
                .WithMessage("Role must be Employee, Manager, or HRAdmin");

            RuleFor(x => x.Gender)
                .Must(gender => string.IsNullOrWhiteSpace(gender) || gender is "Male" or "Female" or "Other")
                .WithMessage("Gender must be Male, Female, or Other");
        }
    }
}
