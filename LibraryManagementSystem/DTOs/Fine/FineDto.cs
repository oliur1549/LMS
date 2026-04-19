namespace LibraryManagementSystem.DTOs.Fine
{
    public class FineDto
    {
        public int Id { get; set; }
        public int BorrowRecordId { get; set; }
        public int MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public int OverdueDays { get; set; }
        public decimal DailyRate { get; set; }
        public decimal Amount { get; set; }
        public DateTime IssuedDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public bool IsPaid { get; set; }
    }
}
