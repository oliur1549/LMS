using FluentValidation;

namespace LibraryManagementSystem.DTOs.Reservation
{
    public class CreateReservationDto
    {
        public int MemberId { get; set; }
        public int BookId { get; set; }
    }

    public class CreateReservationDtoValidator : AbstractValidator<CreateReservationDto>
    {
        public CreateReservationDtoValidator()
        {
            RuleFor(x => x.MemberId)
                .GreaterThan(0).WithMessage("A valid Member is required.");

            RuleFor(x => x.BookId)
                .GreaterThan(0).WithMessage("A valid Book is required.");
        }
    }
}
