using FluentValidation;

namespace LibraryManagementSystem.DTOs.Book
{
    public class UpdateBookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int PublishedYear { get; set; }
        public int TotalCopies { get; set; }
        public int LibraryId { get; set; }
    }

    public class UpdateBookDtoValidator : AbstractValidator<UpdateBookDto>
    {
        public UpdateBookDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("A valid Book ID is required.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(300).WithMessage("Title cannot exceed 300 characters.");

            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Author is required.")
                .MaximumLength(200).WithMessage("Author cannot exceed 200 characters.");

            RuleFor(x => x.ISBN)
               .NotEmpty().WithMessage("ISBN is required.")
               .MaximumLength(20).WithMessage("ISBN cannot exceed 20 characters.");

            RuleFor(x => x.Genre)
                .MaximumLength(100).WithMessage("Genre cannot exceed 100 characters.");

            RuleFor(x => x.PublishedYear)
                .InclusiveBetween(1000, 2100).WithMessage("Published year must be between 1000 and 2100.");

            RuleFor(x => x.TotalCopies)
                .GreaterThan(0).WithMessage("Total copies must be at least 1.");

            RuleFor(x => x.LibraryId)
               .GreaterThan(0).WithMessage("A valid Library is required.");
        }
    }
}
