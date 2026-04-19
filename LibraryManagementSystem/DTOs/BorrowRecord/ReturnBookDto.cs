using FluentValidation;

namespace LibraryManagementSystem.DTOs.BorrowRecord
{
    public class ReturnBookDto
    {
        public int BorrowRecordId { get; set; }
    }

    public class ReturnBookDtoValidator : AbstractValidator<ReturnBookDto>
    {
        public ReturnBookDtoValidator()
        {
            RuleFor(x => x.BorrowRecordId)
                .GreaterThan(0).WithMessage("A valid borrow record is required.");
        }
    }
}
