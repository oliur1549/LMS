using FluentValidation;

namespace LibraryManagementSystem.DTOs.Fine
{
    public class CreateFineDto
    {
        public int BorrowRecordId { get; set; }
        public decimal DailyRate { get; set; } = 5.00m;
    }

    public class CreateFineDtoValidator : AbstractValidator<CreateFineDto>
    {
        public CreateFineDtoValidator()
        {
            RuleFor(x => x.BorrowRecordId)
                .GreaterThan(0).WithMessage("A valid Borrow Record is required.");

            RuleFor(x => x.DailyRate)
                .GreaterThan(0).WithMessage("Daily rate must be greater than 0.");
        }
    }
}
