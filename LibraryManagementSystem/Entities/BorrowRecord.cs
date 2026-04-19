using LibraryManagementSystem.Enums;

namespace LibraryManagementSystem.Entities
{
    public class BorrowRecord : BaseEntity
    {
        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public int MemberId { get; set; }
        public Member Member { get; set; } = null!;

        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public BorrowStatus Status { get; set; } = BorrowStatus.Borrowed;

        // Navigation Property
        public Fine? Fine { get; set; }

        // Encapsulation: Business logic inside the model
        public bool IsOverdue() => Status == BorrowStatus.Borrowed && DateTime.UtcNow > DueDate;

        public void MarkAsReturned()
        {
            ReturnDate = DateTime.UtcNow;
            Status = BorrowStatus.Returned;
        }

        public void CheckAndUpdateOverdue()
        {
            if (IsOverdue())
                Status = BorrowStatus.Overdue;
        }
    }
}
