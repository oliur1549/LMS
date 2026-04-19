namespace LibraryManagementSystem.Entities
{
    public class Book : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int PublishedYear { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }

        // Foreign Key (Many-to-One: Book belongs to one Library)
        public int LibraryId { get; set; }
        public Library Library { get; set; } = null!;

        // Navigation Property (One-to-Many: Book can have many BorrowRecords)
        public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();

        // Navigation Property (One-to-Many: Book can have many Reservations)
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        // Encapsulation: Business logic inside the model
        public bool IsAvailable() => AvailableCopies > 0;

        public void BorrowCopy()
        {
            if (!IsAvailable())
                throw new InvalidOperationException("No available copies to borrow.");
            AvailableCopies--;
        }

        public void ReturnCopy()
        {
            if (AvailableCopies >= TotalCopies)
                throw new InvalidOperationException("All copies are already returned.");
            AvailableCopies++;
        }
    }
}
