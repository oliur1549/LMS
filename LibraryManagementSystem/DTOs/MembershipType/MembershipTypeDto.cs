namespace LibraryManagementSystem.DTOs.MembershipType
{
    public class MembershipTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MaxBooksAllowed { get; set; }
        public int BorrowDurationDays { get; set; }
    }
}
