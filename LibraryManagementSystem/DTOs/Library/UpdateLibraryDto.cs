using FluentValidation;

namespace LibraryManagementSystem.DTOs.Library
{
    public class UpdateLibraryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateLibraryDtoValidator : AbstractValidator<UpdateLibraryDto>
    {
        public UpdateLibraryDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("A valid Library ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Library name is required.")
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.")
                .MaximumLength(500).WithMessage("Address cannot exceed 500 characters.");

            RuleFor(x => x.Phone)
                .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("A valid email address is required.")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.Email));
        }
    }
}
