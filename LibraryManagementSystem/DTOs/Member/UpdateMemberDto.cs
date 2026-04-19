using FluentValidation;

namespace LibraryManagementSystem.DTOs.Member
{
    public class UpdateMemberDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int MembershipTypeId { get; set; }
    }

    public class UpdateMemberDtoValidator : AbstractValidator<UpdateMemberDto>
    {
        public UpdateMemberDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("A valid Member ID is required.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(200).WithMessage("Full name cannot exceed 200 characters.");

            RuleFor(x => x.Phone)
                .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.");

            RuleFor(x => x.MembershipTypeId)
                .GreaterThan(0).WithMessage("A valid Membership Type is required.");
        }
    }
}
