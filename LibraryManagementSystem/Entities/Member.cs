namespace LibraryManagementSystem.Entities
{
    public class Member : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime MembershipDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Foreign Key (Many-to-One: Member belongs to one MembershipType)
        public int MembershipTypeId { get; set; }
        public MembershipType MembershipType { get; set; } = null!;

        // Navigation Property (One-to-Many: Member can have many BorrowRecords)
        public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();

        // Navigation Property (One-to-Many: Member can have many Reservations)
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        // Navigation Property (One-to-Many: Member can have many Fines)
        public ICollection<Fine> Fines { get; set; } = new List<Fine>();

        // Encapsulation: Business logic inside the model
        public bool CanBorrow() => IsActive && !IsDeleted;

        public void Deactivate() => IsActive = false;

        public void Activate() => IsActive = true;
    }
}
