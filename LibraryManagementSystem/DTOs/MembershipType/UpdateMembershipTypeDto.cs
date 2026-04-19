using FluentValidation;

namespace LibraryManagementSystem.DTOs.MembershipType
{
    public class UpdateMembershipTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MaxBooksAllowed { get; set; }
        public int BorrowDurationDays { get; set; }
    }

    public class UpdateMembershipTypeDtoValidator : AbstractValidator<UpdateMembershipTypeDto>
    {
        public UpdateMembershipTypeDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("A valid Membership Type ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Membership type name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.MaxBooksAllowed)
                .GreaterThan(0).WithMessage("Max books allowed must be at least 1.");

            RuleFor(x => x.BorrowDurationDays)
                .GreaterThan(0).WithMessage("Borrow duration must be at least 1 day.");
        }
    }
}
