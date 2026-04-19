namespace LibraryManagementSystem.Entities
{
    public class Fine : BaseEntity
    {
        public int BorrowRecordId { get; set; }
        public BorrowRecord BorrowRecord { get; set; } = null!;

        public int MemberId { get; set; }
        public Member Member { get; set; } = null!;

        public int OverdueDays { get; set; }
        public decimal DailyRate { get; set; }
        public decimal Amount { get; set; }
        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;
        public DateTime? PaidDate { get; set; }
        public bool IsPaid { get; set; } = false;

        public void MarkAsPaid()
        {
            if (IsPaid)
                throw new InvalidOperationException("This fine has already been paid.");
            IsPaid = true;
            PaidDate = DateTime.UtcNow;
        }
    }
}
