using FluentValidation;

namespace LibraryManagementSystem.DTOs.BorrowRecord
{
    public class BorrowBookDto
    {
        public int BookId { get; set; }
        public int MemberId { get; set; }
    }

    public class BorrowBookDtoValidator : AbstractValidator<BorrowBookDto>
    {
        public BorrowBookDtoValidator()
        {
            RuleFor(x => x.BookId)
                .GreaterThan(0).WithMessage("A valid Book is required.");

            RuleFor(x => x.MemberId)
                .GreaterThan(0).WithMessage("A valid Member is required.");
        }
    }
}
